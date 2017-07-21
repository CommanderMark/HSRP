using System;

namespace HSRP.Commands
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    class AbilityAttribute : Attribute
    {
        public AbilityType Type;
        public bool Offense;
        public string Desc;

        public AbilityAttribute(AbilityType type, bool offense, string desc)
        {
            this.Type = type;
            this.Offense = offense;
            this.Desc = desc;
        }

        public override string ToString() => Type.ToString() + " " + (Offense ? "Offense" : "Defense");
    }
}