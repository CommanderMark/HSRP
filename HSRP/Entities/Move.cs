﻿using System.Collections.Generic;
using System.Xml.Linq;

namespace HSRP
{
    public class Move
    {
        public string Name;
        public int Priority;

        private bool usesRolls;
        private string[] attackerRolls;
        private string[] targetRolls;

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

        private Move()
        {
            Name = string.Empty;

            usesRolls = false;
            attackerRolls = new string[0];
            targetRolls = new string[0];

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
                    case "rolls":
                    {
                        usesRolls = true;
                        attackerRolls = element.GetAttributeStringArray("atk", new string[0]);
                        targetRolls = element.GetAttributeStringArray("tar", new string[0]);
                    }
                    break;

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
            
            if (usesRolls)
            {
                XElement rolls = new XElement("rolls",
                    new XAttribute("atk", string.Join(",", attackerRolls)),
                    new XAttribute("tar", string.Join(",", targetRolls))
                    );
                
                move.Add(rolls);
            }

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
            if (!string.IsNullOrWhiteSpace(attackMsg))
            {
                strife.Log.AppendLine();
                strife.Log.AppendLine(Entity.GetEntityMessage(attackMsg, Syntax.ToCodeLine(ent.Name), Syntax.ToCodeLine(tar.Name)));
            }

            if (usesRolls)
            {
                int atkY = 0;
                foreach (string ab in attackerRolls)
                {
                    atkY += ent.GetAbilityValue(ab);
                }

                int tarY = 0;
                foreach (string ab in targetRolls)
                {
                    tarY += ent.GetAbilityValue(ab);
                }

                // Dice rolls.
                int atkRoll = Toolbox.DiceRoll(1, atkY);
                int tarRoll = Toolbox.DiceRoll(1, tarY);
                strife.Log.AppendLine($"{Syntax.ToCodeLine(ent.Name)} rolls {Syntax.ToCodeLine(atkRoll)}!");
                strife.Log.AppendLine($"{Syntax.ToCodeLine(tar.Name)} rolls {Syntax.ToCodeLine(tarRoll)}!");

                if (atkRoll <= tarRoll)
                {
                    strife.Log.AppendLine("Attack missed.");
                    return;
                }
            }

            foreach (Event evnt in events)
            {
                evnt.Fire(ent, tar, attackTeam, strife);
            }

            Cooldown = cooldownMaxTime;
        }
    }
}
