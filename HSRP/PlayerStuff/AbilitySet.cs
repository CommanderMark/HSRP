using System;
using System.Reflection;
using System.Xml.Linq;

namespace HSRP
{
    public class AbilitySet
    {
        public int Strength { get; set; }
        public int Dexterity { get; set; }
        public int Constitution { get; set; }
        public int Intelligence { get; set; }
        public int Wisdom { get; set; }
        public int Charisma { get; set; }

        public static AbilitySet operator +(AbilitySet set1, AbilitySet set2)
        {
            AbilitySet newSet = new AbilitySet();
            newSet.Strength = set1.Strength + set2.Strength;
            newSet.Dexterity = set1.Dexterity + set2.Dexterity;
            newSet.Constitution = set1.Constitution + set2.Constitution;
            newSet.Intelligence = set1.Intelligence + set2.Intelligence;
            newSet.Wisdom = set1.Wisdom + set2.Wisdom;
            newSet.Charisma = set1.Charisma + set2.Charisma;

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

        public string Display()
        {
            AbilitySet modifiers = GetModifiers();
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