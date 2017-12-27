using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace HSRP
{
    public class NPC : IEntity
    {
        public ulong ID { get; set; }
        public string Title;
        public string Name
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

        public bool LikesPineappleOnPizza { get; set; }

        public AbilitySet BaseAbilities { get; set; }

        public Dictionary<EventType, Event> Events { get; set; }
        public string[] Immunities { get; set; }
        public List<StatusEffect> InflictedAilments { get; set; }
        public List<Move> Moves { get; set; }

        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public bool Dead { get; set; }

        public string Specibus { get; set; }
        //TODO: Is this actually needed?
        public Item EquippedWeapon { get; }

        /// <summary>
        /// The amount of times the NPC gets to roll a die for their attack or defense turn.
        /// </summary>
        public int DiceRolls { get; set; }

        public NPC()
        {
            BaseAbilities = new AbilitySet();

            Events = new Dictionary<EventType, Event>();
            Immunities = null;
            InflictedAilments = new List<StatusEffect>();
            Moves = new List<Move>();

            Name = "";
            Description = "";
            Specibus = "";
        }

        public NPC(XElement element) : this()
        {
            ID = XmlToolbox.GetAttributeUnsignedLong(element, "id", 0);

            foreach (XElement ele in element.Elements())
            {
                switch (ele.Name.LocalName)
                {
                    case "info":
                        Name = XmlToolbox.GetAttributeString(ele, "name", string.Empty);
                        Type = XmlToolbox.GetAttributeEnum(ele, "type", NPCType.Normal);
                        LikesPineappleOnPizza = XmlToolbox.GetAttributeBool(ele, "pineappleOnPizza", false);
                        Description = XmlToolbox.ElementInnerText(ele);
                        break;

                    case "status":
                        Health = XmlToolbox.GetAttributeInt(ele, "hp", -1);
                        MaxHealth = XmlToolbox.GetAttributeInt(ele, "maxhp", Health);
                        Dead = XmlToolbox.GetAttributeBool(ele, "dead", false);
                        Specibus = XmlToolbox.GetAttributeString(ele, "specibus", string.Empty);
                        DiceRolls = XmlToolbox.GetAttributeInt(ele, "diceRolls", 1);
                        Immunities = XmlToolbox.GetAttributeStringArray(ele, "immune", new string[0]);
                        break;
                    
                    case "abilities":
                        BaseAbilities = new AbilitySet(ele);
                        break;
                    case "ailments":
                        //TODO: events
                        foreach (XElement strifeEle in ele.Elements("ailment"))
                        {
                            string ailName = XmlToolbox.GetAttributeString(strifeEle, "name", string.Empty);
                            ulong ailController = XmlToolbox.GetAttributeUnsignedLong(strifeEle, "controller", 0);
                            int ailTurns = XmlToolbox.GetAttributeInt(strifeEle, "turns", 0);

                            if (StatusEffect.TryParse(ailName, out StatusEffect sa, ailController, ailTurns))
                            {
                                InflictedAilments.Add(sa);
                            }
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

            XElement abilities = BaseAbilities.ToXmlElement();
            
            // TODO: events.
            XElement ailments = new XElement("ailments");
            foreach (StatusEffect sa in InflictedAilments)
            {
                XElement ailEle = sa.Save();
                ailments.Add(ailEle);
            }

            npc.Add(info, status, abilities, ailments);
            return npc;
        }

        public string Display(bool showMods = false)
        {
            StringBuilder result = new StringBuilder();

            result.AppendLine("Name: " + Name);
            result.AppendLine("Type: " + Type.ToString());
            result.AppendLine("");
            
            result.AppendLine("Description: " + Description);
            result.AppendLine("");

            result.AppendLine("Health Vial: " + Health + "/" + MaxHealth);
            result.AppendLine("Strife Specibus: " + Specibus);
            result.AppendLine("Dice Rolls: " + DiceRolls);
            result.AppendLine("");

            result.AppendLine("Base Statistics");
            result.AppendLine(showMods
                ? BaseAbilities.Display(this.GetModifiers())
                : BaseAbilities.Display());

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
    }
}
