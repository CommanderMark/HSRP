using System;
using System.ComponentModel;
using System.Reflection;
using System.Xml.Linq;

namespace HSRP
{
    public class AbilitySet
    {
        [Description("A person")]
        public int Strength { get; set; }
        // Physical Defense
        public int Constitution { get; set; }
        // Mental Offense
        public int Psion { get; set; }
        // Mental Defense
        public int Fortitude { get; set; }
        // Speech Offense
        public int Intimidation { get; set; }
        // Speech Defense
        public int Persuation { get; set; }

        public static AbilitySet operator +(AbilitySet set1, AbilitySet set2)
        {
            AbilitySet newSet = new AbilitySet();
            newSet.Strength = set1.Strength + set2.Strength;
            newSet.Constitution = set1.Constitution + set2.Constitution;
            newSet.Psion = set1.Psion + set2.Psion;
            newSet.Fortitude = set1.Fortitude + set2.Fortitude;
            newSet.Intimidation = set1.Intimidation + set2.Intimidation;
            newSet.Persuation = set1.Persuation + set2.Persuation;

            return newSet;
        }

        public AbilitySet() { }
        public AbilitySet(XElement ele)
        {
            Type type = this.GetType();
            foreach (PropertyInfo property in type.GetProperties())
            {
                if (property.CanWrite)
                {
                    int value = XmlToolbox.GetAttributeInt(ele.Element(property.Name.ToLower()), "value", 0);
                    property.SetValue(this, value);
                }
            }
        }

        public XElement ToXmlElement()
        {
            XElement ele = new XElement("abilities");
            Type type = this.GetType();
            foreach (PropertyInfo property in type.GetProperties())
            {
                if (property.CanRead)
                {
                    int value = (int)property.GetValue(this);
                    ele.Add(
                        new XElement(property.Name.ToLower(),
                            new XAttribute("value", value)
                            )
                        );
                }
            }

            return ele;
        }

        public AbilitySet GetModifiers()
        {
            AbilitySet modifierSet = new AbilitySet();

            Type type = this.GetType();
            foreach (PropertyInfo property in type.GetProperties())
            {
                if (property.CanWrite && property.CanRead)
                {
                    int value = (int)property.GetValue(this);
                    // y = 0.5x - 5, rounded down.
                    float modiVal = (float)Math.Floor((0.5 * value) - 5);

                    property.SetValue(modifierSet, (int)modiVal);
                }
            }

            return modifierSet;
        }

        public string Display(AbilitySet modifiers)
        {
            string disp = "";

            Type type = this.GetType();
            foreach (PropertyInfo property in type.GetProperties())
            {
                if (property.CanRead)
                {
                    int value = (int)property.GetValue(this);
                    int modiVal = (int)property.GetValue(modifiers);

                    disp += $"{property.Name}: {value} ({modiVal})\n";
                }
            }

            return disp;
        }
    }
}