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

        public int Value;
        public float Percentage;
        
        public Ability() { }
        public Ability(XElement element)
        {
            Value = element.GetAttributeInt("value", 0);
            Percentage = element.GetAttributeFloat("per", 1.0f);
        }

        public XElement Save(string name)
        {
            XElement ability = new XElement(name, new XAttribute("value", Value));

            return ability;
        }
    }
}