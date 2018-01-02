using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;

namespace HSRP
{
    /// <summary>
    /// A class for handing event triggers in a strife.
    /// </summary>
    public class Event
    {
        public static Event WakeUpAfterHit
        {
            get
            {
                Event evnt = new Event();
                evnt.probability = 0.25f;
                evnt.removeEffects.Add(new Tuple<TargetType, string>(TargetType.Self, Constants.SLEEPING_AIL));

                return evnt;
            }
        }

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

            damageTarget = TargetType.None;
            damageAmount = 0f;
            healTarget = TargetType.None;
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
                        TargetType[] enumArr = ele.GetAttributeEnumArray("type", new TargetType[2]);
                        foreach(TargetType enumi in enumArr)
                        {
                            damageTarget |= enumi;
                        }
                    }
                    break;

                    case "healDamage":
                    {
                        healAmount = ele.GetAttributeFloat("amount", 0f);
                        TargetType[] enumArr = ele.GetAttributeEnumArray("type", new TargetType[2]);
                        foreach(TargetType enumi in enumArr)
                        {
                            healTarget |= enumi;
                        }
                    }
                    break;

                    case "ailment":
                    {
                        string name = ele.GetAttributeString("name", string.Empty);
                        TargetType type = TargetType.None;
                        TargetType[] enumArr = ele.GetAttributeEnumArray("type", new TargetType[2]);
                        foreach(TargetType enumi in enumArr)
                        {
                            type |= enumi;
                        }

                        statusEffects.Add(new Tuple<TargetType, string>(type, name));
                    }
                    break;

                    case "cure":
                    {
                        string name = ele.GetAttributeString("name", string.Empty);
                        TargetType type = TargetType.None;
                        TargetType[] enumArr = ele.GetAttributeEnumArray("type", new TargetType[2]);
                        foreach(TargetType enumi in enumArr)
                        {
                            type |= enumi;
                        }

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
                new XAttribute("type", String.Join(",", damageTarget.GetIndividualFlags()))
                );

            XElement healDamage = new XElement("healDamage",
                new XAttribute("amount", healAmount),
                new XAttribute("type", String.Join(",", healTarget.GetIndividualFlags()))
                );

            eventEle.Add(inflictDamage, healDamage);

            foreach (Tuple<TargetType, string> tup in statusEffects)
            {
                eventEle.Add(new XElement("ailment",
                                          new XAttribute("name", tup.Item2),
                                          new XAttribute("type", String.Join(",", tup.Item1.GetIndividualFlags()))
                                         )
                            );
            }

            foreach (Tuple<TargetType, string> tup in removeEffects)
            {
                eventEle.Add(new XElement("cure",
                                          new XAttribute("name", tup.Item2),
                                          new XAttribute("type", String.Join(",", tup.Item1.GetIndividualFlags()))
                                         )
                            );
            }

            if (!string.IsNullOrWhiteSpace(message))
            {
                XElement msg = new XElement("message",
                                        new XText(message)
                                       );
                eventEle.Add(msg);
            }

            return eventEle;
        }

        public override bool Equals(Object obj)
        {
            // If parameter cannot be cast to Point return false.
            Event evnt = obj as Event;
            if (evnt == null)
            {
                return false;
            }

            foreach (PropertyInfo property in GetType().GetProperties())
            {
                if (property.CanWrite && property.CanRead)
                {
                    Object val1 = property.GetValue(this);
                    Object val2 = property.GetValue(evnt);

                    if (val1 != val2)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Fires the event.
        /// </summary>
        /// <param name="ent">The entity this event is attached to.</param>
        /// <param name="tar">The entity the user was targeting or targeted by when the event was triggered.</param>
        /// <param name="attackTeam">Boolean stating whether the entity is on the attacking team or not.</param>
        /// <param name="strife">The strife object itself.</param>
        public void Fire(Entity ent, Entity tar, bool attackTeam, Strife strife)
        {
            // Probability to fire.
            float magicNumber = Toolbox.RandFloat(0f, 1.0f);
            if (magicNumber > probability)
            {
                return;
            }

            int dmgEnt = 0, dmgTar = 0;
            int healEnt = 0, healTar = 0;

            if (damageAmount > 0)
            {
                switch (damageTarget)
                {
                    case TargetType.Self:
                    {
                        dmgEnt = ent.InflictDamageByPercentage(damageAmount);
                    }
                    break;

                    case TargetType.Target:
                    {
                        dmgTar = tar.InflictDamageByPercentage(damageAmount);
                    }
                    break;

                    case TargetType.Self | TargetType.Target:
                    {
                        dmgEnt = ent.InflictDamageByPercentage(healAmount);
                        dmgTar = tar.InflictDamageByPercentage(healAmount);
                    }
                    break;

                    case TargetType.All | TargetType.Self:
                    {
                        foreach (Entity strifer in attackTeam ? strife.Attackers : strife.Targets)
                        {
                            dmgEnt = strifer.InflictDamageByPercentage(damageAmount);
                        }
                    }
                    break;

                    case TargetType.All | TargetType.Target:
                    {
                        foreach (Entity strifer in attackTeam ? strife.Targets : strife.Attackers)
                        {
                            dmgTar = strifer.InflictDamageByPercentage(damageAmount);
                        }
                    }
                    break;

                    case TargetType.All:
                    {
                        foreach (Entity strifer in strife.Entities)
                        {
                            dmgEnt = strifer.InflictDamageByPercentage(damageAmount);
                        }
                    }
                    break;
                }
            }

            if (healAmount > 0)
            {
                switch (healTarget)
                {
                    case TargetType.Self:
                    {
                        healEnt = ent.InflictDamageByPercentage(-healAmount);
                    }
                    break;

                    case TargetType.Target:
                    {
                        healTar = tar.InflictDamageByPercentage(-healAmount);
                    }
                    break;

                    case TargetType.Self | TargetType.Target:
                    {
                        healEnt = ent.InflictDamageByPercentage(-healAmount);
                        healTar = tar.InflictDamageByPercentage(-healAmount);
                    }
                    break;

                    case TargetType.All | TargetType.Self:
                    {
                        foreach (Entity strifer in attackTeam ? strife.Attackers : strife.Targets)
                        {
                            healEnt = strifer.InflictDamageByPercentage(-healAmount);
                        }
                    }
                    break;

                    case TargetType.All | TargetType.Target:
                    {
                        foreach (Entity strifer in attackTeam ? strife.Targets : strife.Attackers)
                        {
                            healTar = strifer.InflictDamageByPercentage(-healAmount);
                        }
                    }
                    break;

                    case TargetType.All:
                    {
                        foreach (Entity strifer in strife.Entities)
                        {
                            healEnt = strifer.InflictDamageByPercentage(-healAmount);
                        }
                    }
                    break;
                }
            }

            strife.Log.AppendLine();
            strife.Log.AppendLine(Entity.GetEntityMessage(message, Syntax.ToCodeLine(ent.Name), Syntax.ToCodeLine(tar.Name), Syntax.ToCodeLine(dmgEnt.ToString()), Syntax.ToCodeLine(dmgTar.ToString()), Syntax.ToCodeLine(healEnt.ToString()), Syntax.ToCodeLine(healTar.ToString())));

            foreach (Tuple<TargetType, string> tup in statusEffects)
            {
                switch (tup.Item1)
                {
                    case TargetType.Self:
                    {
                        ent.ApplyStatusEffect(tup.Item2, tar, attackTeam, strife);
                    }
                    break;

                    case TargetType.Target:
                    {
                        tar.ApplyStatusEffect(tup.Item2, ent, !attackTeam, strife);
                    }
                    break;

                    case TargetType.Self | TargetType.Target:
                    {
                        ent.ApplyStatusEffect(tup.Item2, tar, attackTeam, strife);
                        tar.ApplyStatusEffect(tup.Item2, ent, !attackTeam, strife);
                    }
                    break;

                    case TargetType.All | TargetType.Self:
                    {
                        foreach (Entity strifer in attackTeam ? strife.Attackers : strife.Targets)
                        {
                            strifer.ApplyStatusEffect(tup.Item2, tar, attackTeam, strife);
                        }
                    }
                    break;

                    case TargetType.All | TargetType.Target:
                    {
                        foreach (Entity strifer in attackTeam ? strife.Targets : strife.Attackers)
                        {
                            strifer.ApplyStatusEffect(tup.Item2, ent, !attackTeam, strife);
                        }
                    }
                    break;

                    case TargetType.All:
                    {
                        foreach (Entity strifer in attackTeam ? strife.Attackers : strife.Targets)
                        {
                            strifer.ApplyStatusEffect(tup.Item2, tar, attackTeam, strife);
                        }
                        foreach (Entity strifer in attackTeam ? strife.Targets : strife.Attackers)
                        {
                            strifer.ApplyStatusEffect(tup.Item2, ent, !attackTeam, strife);
                        }
                    }
                    break;
                }
            }

            foreach (Tuple<TargetType, string> tup in removeEffects)
            {
                switch (tup.Item1)
                {
                    case TargetType.Self:
                    {
                        ent.RemoveStatusEffect(tup.Item2, strife, false);
                    }
                    break;

                    case TargetType.Target:
                    {
                        tar.RemoveStatusEffect(tup.Item2, strife, false);
                    }
                    break;

                    case TargetType.Self | TargetType.Target:
                    {
                        ent.RemoveStatusEffect(tup.Item2, strife, false);
                        tar.RemoveStatusEffect(tup.Item2, strife, false);
                    }
                    break;

                    case TargetType.All | TargetType.Self:
                    {
                        foreach (Entity strifer in attackTeam ? strife.Attackers : strife.Targets)
                        {
                            strifer.RemoveStatusEffect(tup.Item2, strife, false);
                        }
                    }
                    break;

                    case TargetType.All | TargetType.Target:
                    {
                        foreach (Entity strifer in attackTeam ? strife.Targets : strife.Attackers)
                        {
                            strifer.RemoveStatusEffect(tup.Item2, strife, false);
                        }
                    }
                    break;

                    case TargetType.All:
                    {
                        foreach (Entity strifer in strife.Entities)
                        {
                            strifer.RemoveStatusEffect(tup.Item2, strife, false);
                        }
                    }
                    break;
                }
            }
        }
    }
}
