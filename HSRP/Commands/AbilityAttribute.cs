using System;

namespace HSRP.Commands
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    class AbilityAttribute : Attribute
    {
        public string Alias;
        public string Desc;

        public AbilityAttribute(string alias, string desc)
        {
            this.Alias = alias;
            this.Desc = desc;
        }
    }
}