using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace HSRP
{
    /// <summary>
    /// A class for handing event triggers in a strife.
    /// </summary>
    public class Event
    {
        /// <summary>
        /// Contains the type of target the status effect should affect and the name of the status effects invoked when this event is triggered.
        /// </summary>
        private List<Tuple<TargetType, string>> statusEffects;

        /// <summary>
        /// Contains the type of target the status effect should affect and the name of the status effects cured (if the target has it) when this event is triggered.
        /// </summary>
        private List<Tuple<TargetType, string>> removeEffects;

        /// <summary>
        /// The entity(ies) that will be targeted by the damageAmount field.
        /// </summary>
        private TargetType damageTarget;
        /// <summary>
        /// Amount of damage to inflict when the event is triggered.
        /// </summary>
        private float damageAmount;

        /// <summary>
        /// The entity(ies) that will be targeted by the healAmount field.
        /// </summary>
        private TargetType healTarget;
        /// <summary>
        /// Amount of damage to inflict when the event is triggered.
        /// </summary>
        private float healAmount;

        /// <summary>
        /// Message that will be sent when the event is triggered.
        /// </summary>
        private string message;

        /// <summary>
        /// The probability of the event firing when the condition is triggered.
        /// Default is 1 (100%).
        /// </summary>
        private float probability;

        private Event()
        {
            statusEffects = new List<Tuple<TargetType, string>>();
            removeEffects = new List<Tuple<TargetType, string>>();

            damageTarget = TargetType.Self;
            damageAmount = 0f;
            healTarget = TargetType.Self;
            healAmount = 0f;

            message = string.Empty;
            probability = 1.0f;
        }

        public Event(XElement element) : this()
        {
            probability = element.GetAttributeFloat("probability", 1.0f);

            foreach (XElement ele in element.Elements())
            {
                switch (ele.Name.LocalName)
                {
                    case "inflictDamage":
                    {
                        damageAmount = ele.GetAttributeFloat("amount", 0f);
                        damageTarget = ele.GetAttributeEnum("type", TargetType.Self);
                    }
                    break;

                    case "healDamage":
                    {
                        healAmount = ele.GetAttributeFloat("amount", 0f);
                        healTarget = ele.GetAttributeEnum("type", TargetType.Self);
                    }
                    break;

                    case "ailment":
                    {
                        string name = ele.GetAttributeString("name", string.Empty);
                        TargetType type = ele.GetAttributeEnum("type", TargetType.Self);

                        statusEffects.Add(new Tuple<TargetType, string>(type, name));
                    }
                    break;

                    case "cure":
                    {
                        string name = ele.GetAttributeString("name", string.Empty);
                        TargetType type = ele.GetAttributeEnum("type", TargetType.Self);

                        removeEffects.Add(new Tuple<TargetType, string>(type, name));
                    }
                    break;

                    case "message":
                    {
                        message = ele.ElementInnerText();
                    }
                    break;
                }
            }
        }

        public XElement Save()
        {
            XElement eventEle = new XElement("event",
                new XAttribute("probability", probability)
                );

            XElement inflictDamage = new XElement("inflictDamage",
                new XAttribute("amount", damageAmount),
                new XAttribute("type", damageTarget.ToString())
                );

            XElement healDamage = new XElement("healDamage",
                new XAttribute("amount", healAmount),
                new XAttribute("type", healTarget.ToString())
                );

            eventEle.Add(inflictDamage, healDamage);

            foreach (Tuple<TargetType, string> tup in statusEffects)
            {
                eventEle.Add(new XElement("ailment",
                                          new XAttribute("name", tup.Item2),
                                          new XAttribute("type", tup.Item1.ToString())
                                         )
                            );
            }

            foreach (Tuple<TargetType, string> tup in removeEffects)
            {
                eventEle.Add(new XElement("cure",
                                          new XAttribute("name", tup.Item2),
                                          new XAttribute("type", tup.Item1.ToString())
                                         )
                            );
            }

            XElement msg = new XElement("message",
                                        new XText(message)
                                       );
            eventEle.Add(msg);

            return eventEle;
        }
    }
}
