using System;
using System.Xml.Linq;

namespace HSRP
{
    public class Ability
    {
        public const string STR = "Strength";
        public const string CON = "Constitution";
        public const string PSI = "Psion";
        public const string FOR = "Fortitude";
        public const string INT = "Intimidation";
        public const string PER = "Persuasion";

        public readonly String name;

        public int Value;
        public float Percentage;
        
        public Ability(string name)
        {
            this.name = name;
        }

        public Ability(Ability copy)
        {
            name = copy.name;
            Value = copy.Value;
            Percentage = copy.Percentage;
        }

        public Ability(XElement element)
        {
            Value = element.GetAttributeInt("value", 0);
            Percentage = element.GetAttributeFloat("per", 0f);
        }

        public XElement Save(string name)
        {
            XElement ability = new XElement(name, new XAttribute("value", Value));
            return ability;
        }

        public bool NameMatch(string name)
        {
            return this.name.Equals(name, StringComparison.OrdinalIgnoreCase);
        }
    }
}