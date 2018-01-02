using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace HSRP
{
    public class Item : IEventable
    {
        public string Name;
        public int Quantity;

        // TODO: XML events
        public List<Tuple<EventType, Event>> Events { set; get; }
        public string[] Immunities { get; set; }

        public Item()
        {
            Name = string.Empty;
            Quantity = 1;

            Events = new List<Tuple<EventType, Event>>();
            Immunities = null;
        }

        public Item(XElement ele) : this()
        {
            Name = XmlToolbox.GetAttributeString(ele, "value", string.Empty);
            Quantity = XmlToolbox.GetAttributeInt(ele, "quantity", 1);
            Immunities = XmlToolbox.GetAttributeStringArray(ele, "immune", new string[0]);
        }

        public XElement Save(bool equipped)
        {
            XElement item = new XElement("item",
                new XAttribute("value", Name),
                new XAttribute("quantity", Quantity)
                );
            if (Immunities.Length > 0)
            {
                item.Add(new XAttribute("immune", string.Join(",", Immunities)));
            }

            return item;
        }
    }
}
