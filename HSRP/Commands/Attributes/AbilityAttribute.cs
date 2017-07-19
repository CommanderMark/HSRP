using System;

namespace HSRP.Commands
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    class AbilityAttribute : Attribute
    {
        public string Alias;
        public string Desc;
        public string Quote;
        public string Author;

        public AbilityAttribute(string alias, string desc)
        {
            this.Alias = alias;
            this.Desc = desc;
        }

        public AbilityAttribute(string alias, string desc, string quote, string author) : this(alias, desc)
        {
            this.Quote = quote;
            this.Author = author;
        }
    }
}