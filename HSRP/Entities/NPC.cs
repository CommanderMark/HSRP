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

        /// <summary>
        /// ID of the character controlling this one, if any.
        /// </summary>
        public ulong Controller { get; set; }

        public NPC()
        {
            BaseAbilities = new AbilitySet();

            Events = new Dictionary<EventType, Event>();
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
                        Controller = XmlToolbox.GetAttributeUnsignedLong(ele, "controller", 0);
                        break;
                    
                    case "abilities":
                        BaseAbilities = new AbilitySet(ele);
                        break;
                    // TODO: Status effects
                    case "modifiers":
                        foreach (XElement strifeEle in ele.Elements())
                        {
                            int? turns = XmlToolbox.GetAttributeNullableInt(strifeEle, "turns", null);
                            if (turns == null)
                            {
                                PermanentModifiers = new AbilitySet(strifeEle);
                            }
                            else
                            {
                                TempMods.Add((int)turns, new AbilitySet(strifeEle));
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
                new XAttribute("diceRolls", DiceRolls),
                new XAttribute("controller", Controller)
                );

            XElement abilities = BaseAbilities.ToXmlElement();

            npc.Add(info, status, abilities);
            // TODO: Status effects.
            if (!TotalAbilities.Equals(Abilities))
            {
                XElement modifiers = new XElement("modifiers");
                modifiers.Add(PermanentModifiers.ToXmlWithoutEmpties());
                foreach (KeyValuePair<int, AbilitySet> mod in TempMods)
                {
                    XElement modEle = mod.Value.ToXmlWithoutEmpties();
                    modEle.Add(new XAttribute("turns", mod.Key));

                    modifiers.Add(modEle);
                }
                npc.Add(modifiers);
            }
            
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
                ? Abilities.Display(TotalMods)
                : Abilities.Display());

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
