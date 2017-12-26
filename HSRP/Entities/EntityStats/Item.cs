using System.Xml.Linq;

namespace HSRP
{
    public class Item
    {
        public string Name;
        public int Quantity;

        // TODO: XML events
        public Events Events;

        public Item()
        {
            Name = string.Empty;
            Quantity = 1;

            Events = new Events();
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
