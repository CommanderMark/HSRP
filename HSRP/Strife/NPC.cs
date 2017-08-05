using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace HSRP
{
    public class NPC : IEntity
    {
        public ulong ID { get; set; }
        public string Name { get; set; }
        public string Title
        {
            get
            {
                if (Type == NPCType.Lusus)
                {
                    return Name + " (Lusus)";
                }

                return Name;
            }
        }
        public string Description { get; set; }
        public NPCType Type { get; set; }

        public bool LikesPineappleOnPizza { get; set; }

        public AbilitySet Abilities { get; set; }

        /// <summary>
        /// Buffs or debuffs applied to stats that remain until the end of the strife.
        /// </summary>
        public AbilitySet Modifiers { get; set; }

        /// <summary>
        /// Buffs or debuffs applied to stats that remain for a specified number of turns.
        /// The key is the number of turns left until the modifier is removed.
        /// </summary>
        public Dictionary<int, AbilitySet> TempMods { get; set; }

        /// <summary>
        /// An AbilitySet containing both the character's base ability stats and their modifiers.
        /// </summary>
        public AbilitySet TotalAbilities
        {
            get
            {
                AbilitySet aSet = Abilities + Modifiers;
                if (TempMods.Any())
                {
                    foreach (KeyValuePair<int, AbilitySet> set in TempMods)
                    {
                        aSet += set.Value;
                    }
                }

                return aSet;
            }

            set
            {
                Abilities = value;
            }
        }

        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public string Specibus { get; set; }
        
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
            Abilities = new AbilitySet();
            TempMods = new Dictionary<int, AbilitySet>();

            Name = "";
            Description = "";
            Specibus = "";
        }

        public NPC(XElement element) : this()
        {
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
                        Specibus = XmlToolbox.GetAttributeString(ele, "specibus", string.Empty);
                        DiceRolls = XmlToolbox.GetAttributeInt(ele, "diceRolls", 1);
                        Controller = XmlToolbox.GetAttributeUnsignedLong(ele, "controller", 0);
                        break;
                    
                    case "abilities":
                        Abilities = new AbilitySet(ele);
                        break;
                    
                    case "strife":
                        foreach (XElement strifeEle in ele.Elements())
                        {
                            int? turns = XmlToolbox.GetAttributeNullableInt(strifeEle, "turns", null);
                            if (turns == null)
                            {
                                Modifiers = new AbilitySet(strifeEle);
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
            XElement npc = new XElement("npc");

            XElement info = new XElement("info",
                new XAttribute("name", Name),
                new XAttribute("type", Type),
                new XAttribute("pineappleOnPizza", LikesPineappleOnPizza),
                new XText(Description)
                );
            
            XElement status = new XElement("status",
                new XAttribute("hp", Health),
                new XAttribute("maxhp", MaxHealth),
                new XAttribute("specibus", Specibus),
                new XAttribute("diceRolls", DiceRolls),
                new XAttribute("controller", Controller)
                );

            XElement abilities = Abilities.ToXmlElement();
                
            XElement strife = new XElement("strife");

            strife.Add(Modifiers.ToXmlElement());
            foreach (KeyValuePair<int, AbilitySet> mod in TempMods)
            {
                XElement modEle = mod.Value.ToXmlElement();
                modEle.Add(new XAttribute("turns", mod.Key));
            }

            npc.Add(info, status, abilities, strife);
            return npc;
        }
    }
}
