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

        public NPC(string filePath) : this()
        {
            XDocument doc = XmlToolbox.TryLoadXml(filePath);
            foreach (XElement ele in doc.Root.Elements())
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
                        break;
                }
            }
        }
    }
}
