using System.Xml.Linq;

namespace HSRP
{
    public class StatusEffect
    {
        // Inflict damage stuff.
        public bool InflictsDamage = false;
        public int MinDamagePercentage = 0;
        public int MaxDamagePercentage = 0;

        // Skip a turn.
        public bool SkipsTurn = false;

        // Mind control.
        public ulong Controller = 0;

        // Stat buffs/debuffs.
        public AbilitySet Modifiers = new AbilitySet();

        // Misc. general status effect stuff.
        public string Name = string.Empty;
        public int Turns = 0;
        public string InflictMsg = string.Empty;
        public string StatusMsg = string.Empty;
        public string EndMsg = string.Empty;

        public StatusEffect(XElement element)
        {
            InflictsDamage = XmlToolbox.GetAttributeBool(element, "canDamage", false);
            SkipsTurn = XmlToolbox.GetAttributeBool(element, "skipTurns", false);
            Controller = XmlToolbox.GetAttributeUnsignedLong(element, "controller", 0);
            Turns = XmlToolbox.GetAttributeInt(element, "turns", 0);

            foreach (XElement ele in element.Elements())
            {
                switch (ele.Name.LocalName)
                {
                    case "inflictDamage":
                    {
                        MinDamagePercentage = XmlToolbox.GetAttributeInt(element, "minAmount", 0);
                        MaxDamagePercentage = XmlToolbox.GetAttributeInt(element, "maxAmount", 0);
                    }
                    break;

                    case "inflictMsg":
                    {
                        InflictMsg = XmlToolbox.ElementInnerText(ele);
                    }
                    break;

                    case "statusMsg":
                    {
                        StatusMsg = XmlToolbox.ElementInnerText(ele);
                    }
                    break;

                    case "endMsg":
                    {
                        EndMsg = XmlToolbox.ElementInnerText(ele);
                    }
                    break;

                    case "abilities":
                    {
                        Modifiers = new AbilitySet(ele);
                    }
                    break;
                }
            }
        }

        public XElement Save()
        {
            XElement ailment = new XElement("ailment",
                new XAttribute("canDamage", InflictsDamage),
                new XAttribute("skipTurns", SkipsTurn),
                new XAttribute("controller", Controller),
                new XAttribute("turns", Turns)
                );

            XElement inflictDamage = new XElement("inflictDamage",
                new XAttribute("minAmount", MinDamagePercentage),
                new XAttribute("maxAmount", MaxDamagePercentage)
                );

            XElement abilities = Modifiers.ToXmlWithoutEmpties();

            XElement inflictMsg = new XElement("inflictMsg",
                new XText(this.InflictMsg)
                );

            XElement statusMsg = new XElement("statusMsg",
                new XText(this.StatusMsg)
                );

            XElement endMsg = new XElement("endMsg",
                new XText(this.EndMsg)
                );

            ailment.Add(inflictDamage, inflictMsg, statusMsg, endMsg);

            if (!Modifiers.Equals(new AbilitySet()))
            {
                ailment.Add(Modifiers);
            }

            return ailment;
        }
    }
}
