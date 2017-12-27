using System.Collections.Generic;
using System.Xml.Linq;

namespace HSRP
{
    public class Item : IEventable
    {
        public string Name;
        public int Quantity;

        // TODO: XML events
        public Dictionary<EventType, Event> Events { set; get; }

        public Item()
        {
            Name = string.Empty;
            Quantity = 1;

            Events = new Dictionary<EventType, Event>();
        }

        public Item(XElement ele) : this()
        {
            Name = XmlToolbox.GetAttributeString(ele, "value", string.Empty);
            Quantity = XmlToolbox.GetAttributeInt(ele, "quantity", 1);
        }

        public XElement Save(bool equipped)
        {
            XElement item = new XElement("item",
                new XAttribute("value", Name),
                new XAttribute("quantity", Quantity)
                );

            return item;
        }
    }
}
