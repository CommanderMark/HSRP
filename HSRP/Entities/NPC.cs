using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace HSRP
{
    public class NPC : Entity
    {
        public string Title;
        public override string Name
        {
            get
            {
                if (Type == NPCType.Lusus)
                {
                    return Title + " (Lusus)";
                }

                return Title;
            }

            set
            {
                Title = value; 
            }
        }
        
        public string Description { get; set; }
        public NPCType Type { get; set; }

        public List<Move> MoveQueue;

        private NPC() : base()
        {
            Description = "";
            MoveQueue = new List<Move>();
        }

        public NPC(XElement element) : this()
        {
            ID = XmlToolbox.GetAttributeUnsignedLong(element, "id", 0);

            foreach (XElement ele in element.Elements())
            {
                switch (ele.Name.LocalName)
                {
                    case "info":
                        Name = ele.GetAttributeString("name", string.Empty);
                        Type = ele.GetAttributeEnum("type", NPCType.Normal);
                        LikesPineappleOnPizza = ele.GetAttributeBool("pineappleOnPizza", false);
                        Description = ele.ElementInnerText();
                        break;

                    case "status":
                        Health = ele.GetAttributeInt("hp", -1);
                        MaxHealth = ele.GetAttributeInt("maxhp", Health);
                        Dead = ele.GetAttributeBool("dead", false);
                        Specibus = ele.GetAttributeString("specibus", string.Empty);
                        DiceRolls = ele.GetAttributeInt("diceRolls", 1);
                        Immunities = ele.GetAttributeStringArray("immune", new string[0]);

                        string[] arr = ele.GetAttributeStringArray("priorities", new string[0]);
                        foreach (string uh in arr)
                        {
                            Move mov = Moves.Values.FirstOrDefault(x => x.Name == uh);
                            if (mov != null)
                            {
                                MoveQueue.Add(mov);
                            }
                        }
                        break;
                    
                    case "abilities":
                        BaseAbilities = new AbilitySet(ele);
                        break;
                    case "ailments":
                        foreach (XElement strifeEle in ele.Elements("ailment"))
                        {
                            string ailName = strifeEle.GetAttributeString("name", string.Empty);

                            if (!StatusEffect.TryParse(ailName, out StatusEffect sa))
                            {
                                sa = new StatusEffect(strifeEle);
                            }
                            else
                            {
                                sa.Controller = strifeEle.GetAttributeUnsignedLong("controller", 0);
                                sa.Turns = strifeEle.GetAttributeInt("turns", 0);
                                XElement abEle = strifeEle.Element("abilities");
                                AbilitySet set = abEle != null
                                    ? new AbilitySet(abEle)
                                    : new AbilitySet();
                                sa.Modifiers = set;
                            }
                            
                            InflictedAilments.Add(sa);
                        }
                        break;

                    case "events":
                        foreach (XElement strifeEle in ele.Elements("event"))
                        {
                            Event evnt = new Event(strifeEle);
                            EventType type = strifeEle.GetAttributeEnum("trigger", EventType.NONE);

                            if (type != EventType.NONE)
                            {
                                Events.Add(new Tuple<EventType, Event>(type, evnt));
                            }
                            else
                            {
                                Console.WriteLine("STRIFE ERROR: Event has invalid type for \"" + this.Name + "\"!");
                            }
                        }
                        break;

                    case "moves":
                        foreach (XElement strifeEle in ele.Elements("move"))
                        {
                            Move evnt = new Move(strifeEle);
                            Moves.Add(evnt.Name, evnt);
                        }
                        break;
                }
            }
        }

        public XElement Save()
        {
            XElement npc = new XElement("npc",
                new XAttribute("id", ID)
                );

            XElement info = new XElement("info",
                new XAttribute("name", Title),
                new XAttribute("type", Type),
                new XAttribute("pineappleOnPizza", LikesPineappleOnPizza),
                new XText(Description)
                );

            XElement status = new XElement("status",
                new XAttribute("hp", Health),
                new XAttribute("maxhp", MaxHealth),
                new XAttribute("dead", Dead),
                new XAttribute("specibus", Specibus),
                new XAttribute("diceRolls", DiceRolls)
                );
            if (Immunities.Length > 0)
            {
                status.Add(new XAttribute("immune", string.Join(",", Immunities)));
            }

            if (MoveQueue.Count() > 0)
            {
                status.Add(new XAttribute("priorities", string.Join(",", MoveQueue.Select(x => x.Name))));
            }

            XElement abilities = BaseAbilities.ToXmlElement();
            
            XElement ailments = new XElement("ailments");
            foreach (StatusEffect sa in InflictedAilments)
            {
                XElement ailEle = sa.Save();
                ailments.Add(ailEle);
            }

            npc.Add(info, status, abilities, ailments);

            XElement events = new XElement("events");
            foreach (Tuple<EventType, Event> evnt in Events)
            {
                if (evnt.Item2.Equals(Event.WakeUpAfterHit)) { continue; }
                XElement ailEle = evnt.Item2.Save();
                ailEle.Add(new XAttribute("trigger", evnt.Item1.ToString()));

                events.Add(ailEle);
            }
            if (events.Elements().Count() > 0)
            {
                npc.Add(events);
            }

            XElement moves = new XElement("moves");
            foreach (KeyValuePair<string, Move> mov in Moves)
            {
                moves.Add(mov.Value.Save());
            }
            if (moves.Elements().Count() > 0)
            {
                npc.Add(moves);
            }

            return npc;
        }

        public override string Display(bool showMods = false)
        {
            StringBuilder result = new StringBuilder();

            result.AppendLine("Name: " + Name);
            result.AppendLine("Type: " + Type.ToString());
            result.AppendLine();
            
            result.AppendLine("Description: " + Description);
            result.AppendLine();

            result.AppendLine("Health Vial: " + Health + "/" + MaxHealth);
            result.AppendLine("Strife Specibus: " + Specibus);
            result.AppendLine("Dice Rolls: " + DiceRolls);
            result.AppendLine();

            result.AppendLine("Base Statistics");
            result.AppendLine(showMods
                ? BaseAbilities.Display(this.GetTotalAbilities())
                : BaseAbilities.Display());
            
            if (showMods)
            {
                result.AppendLine();
                result.Append("Status Effects: ");
                IEnumerable<StatusEffect> lis = from x in InflictedAilments
                                        where x.Name != Constants.PHYSICAL_COUNTER_AIL && x.Name != Constants.SPE_ATTACK_AIL
                                        select x;
                if (lis.Count() > 0)
                {
                    result.Append(lis.ElementAt(0).Name);
                    foreach (StatusEffect sa in lis)
                    {
                        result.Append(", " + sa.Name);
                    }
                }
                else
                {
                    result.Append("None");
                }
            }

            return result.ToString();
        }

        public static bool TryParse(string input, out NPC npc, bool idOnly = true)
        {
            string filePath = idOnly
                ? Path.Combine(Dirs.NPCs, input) + ".xml"
                : input;
            
            if (File.Exists(filePath))
            {
                XDocument doc = XmlToolbox.TryLoadXml(filePath);
                if (doc != null)
                { 
                    npc = new NPC(doc.Root);
                    return true; 
                }
            }

            npc = null;
            return false;
        }

        /// <summary>
        /// Generates a priority list for the NPC to dictate what moves they should use.
        /// </summary>
        public void GenerateMoveList()
        {
            for (int i = 0; i < Moves.Count(); i++)
            {
                Move mov = Moves.ElementAt(i).Value;

                if (mov.Priority <= 0)
                {
                    MoveQueue.Add(mov);
                }
                // Add the index the same amount of times as the priority number.
                else
                {
                    for (int j = 0; j < mov.Priority; j++)
                    {
                        MoveQueue.Add(mov);
                    }
                }
            }

            Shuffle(MoveQueue);
        }

        private static void Shuffle<T>(List<T> list)  
        {  
            int n = list.Count;  
            while (n > 1)
            {  
                n--;  
                int k = Toolbox.RandInt(n);  
                T value = list[k];  
                list[k] = list[n];  
                list[n] = value;  
            }  
        }
    }
}
