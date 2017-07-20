using System.Xml.Linq;

namespace HSRP
{
    public class NPC : IEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public bool LikesPineappleOnPizza { get; set; }

        public AbilitySet Abilities { get; set; }

        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public string Specibus { get; set; }
        /// <summary>
        /// The amount of times the NPC gets to roll a die for their attack turn.
        /// </summary>
        public int DiceRolls { get; set; }

        public NPC()
        {
            Abilities = new AbilitySet();

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
                        LikesPineappleOnPizza = XmlToolbox.GetAttributeBool(ele, "pineappleOnPizza", false);
                        Description = XmlToolbox.ElementInnerText(ele);
                        break;

                    case "status":
                        Health = XmlToolbox.GetAttributeInt(ele, "hp", -1);
                        MaxHealth = XmlToolbox.GetAttributeInt(ele, "maxhp", Health);
                        Specibus = XmlToolbox.GetAttributeString(ele, "specibus", string.Empty);
                        DiceRolls = XmlToolbox.GetAttributeInt(ele, "diceRolls", 1);
                        break;
                    
                    case "abilities":
                        Abilities = new AbilitySet(ele);
                        break;
                }
            }
        }

        public XElement Save()
        {
            XElement npc = new XElement("npc");

            XElement info = new XElement("info",
                new XAttribute("name", Name),
                new XAttribute("pineappleOnPizza", LikesPineappleOnPizza),
                new XText(Description)
                );
            
            XElement status = new XElement("status",
                new XAttribute("hp", Health),
                new XAttribute("maxhp", MaxHealth),
                new XAttribute("specibus", Specibus),
                new XAttribute("diceRolls", DiceRolls)
                );

            XElement abilities = Abilities.ToXmlElement();

            npc.Add(info, status, abilities);
            return npc;
        }
    }
}
