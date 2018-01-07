using System.Collections.Generic;
using System.Xml.Linq;

namespace HSRP
{
    public class Move
    {
        public string Name;
        /// <summary>
        /// Amount of turns to set the cooldown timer to.
        /// </summary>
        private int cooldownMaxTime;
        /// <summary>
        /// The amount of turns left for the move to cooldown.
        /// </summary>
        public int Cooldown;
        private List<Event> events;

        private string attackMsg;
        public string RechargeMsg;

        public Move()
        {
            Name = string.Empty;
            cooldownMaxTime = 0;
            Cooldown = 0;
            events = new List<Event>();

            attackMsg = string.Empty;
            RechargeMsg = string.Empty;
        }

        public Move(XElement element) : this()
        {
            Name = element.GetAttributeString("name", string.Empty);
            cooldownMaxTime = element.GetAttributeInt("cooldownMaxTime", 0);
            Cooldown = element.GetAttributeInt("cooldown", 0);

            foreach (XElement ele in element.Elements())
            {
                switch (ele.Name.LocalName)
                {
                    case "attackMsg":
                    {
                        attackMsg = ele.ElementInnerText();
                    }
                    break;

                    case "rechargeMsg":
                    {
                        RechargeMsg = ele.ElementInnerText();
                    }
                    break;

                    case "event":
                    {
                        events.Add(new Event(ele));
                    }
                    break;
                }
            }
        }

        public XElement Save()
        {
            XElement move = new XElement("move",
                new XAttribute("name", Name),
                new XAttribute("cooldownMaxTime", cooldownMaxTime),
                new XAttribute("cooldown", Cooldown)
                );

            XElement attackEle = new XElement("attackMsg", new XText(attackMsg));
            XElement rechargeEle = new XElement("rechargeMsg", new XText(RechargeMsg));

            move.Add(attackEle, rechargeEle);

            foreach (Event evnt in events)
            {
                move.Add(evnt.Save());
            }

            return move;
        }

        /// <summary>
        /// Applies the move.
        /// </summary>
        /// <param name="ent">The entity this event is attached to.</param>
        /// <param name="tar">The entity the user was targeting or targeted by when the event was triggered.</param>
        /// <param name="attackTeam">Boolean stating whether the entity is on the attacking team or not.</param>
        /// <param name="strife">The strife object itself.</param>
        public void Apply(Entity ent, Entity tar, bool attackTeam, Strife strife)
        {
            foreach (Event evnt in events)
            {
                evnt.Fire(ent, tar, attackTeam, strife);
            }
        }
    }
}
