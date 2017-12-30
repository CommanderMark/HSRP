using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace HSRP
{
    // TODO: Add thing for preventing moves.
    public class StatusEffect
    {
        // Inflict damage stuff.
        private bool inflictsDamage = false;
        public float MinDamagePercentage = 0f;
        public float MaxDamagePercentage = 0f;

        // Skip a turn.
        private bool skipsTurn = false;

        // Mind control.
        public ulong Controller = 0;

        // Explosion
        public bool Explodes = false;
        public Explosion Explosion = new Explosion();

        // Stat buffs/debuffs.
        public AbilitySet Modifiers = new AbilitySet();

        public string[] Immunities;

        // Misc. general status effect stuff.
        public string Name = string.Empty;
        public int Turns = 0;
        public string InflictMsg = string.Empty;
        public string StatusMsg = string.Empty;
        public string EndMsg = string.Empty;

        //TODO:1`00% next to.
        public const int EXPLOSION_FALLOFF_FACTOR = 2;

        public StatusEffect(XElement element)
        {
            Name = element.GetAttributeString("name", string.Empty);
            inflictsDamage = element.GetAttributeBool("canDamage", false);
            skipsTurn = element.GetAttributeBool("skipTurns", false);
            Controller = XmlToolbox.GetAttributeUnsignedLong(element, "controller", 0);
            Explodes = element.GetAttributeBool("explodes", false);
            Turns = element.GetAttributeInt("turns", 0);
            Immunities = XmlToolbox.GetAttributeStringArray(element, "immune", new string[0]);

            foreach (XElement ele in element.Elements())
            {
                switch (ele.Name.LocalName)
                {
                    case "inflictDamage":
                    {
                        MinDamagePercentage = ele.GetAttributeFloat("minAmount", 0f);
                        MaxDamagePercentage = ele.GetAttributeFloat("maxAmount", 0f);
                    }
                    break;

                    case "explosion":
                    {
                        Explosion = new Explosion(ele);
                    }
                    break;

                    case "inflictMsg":
                    {
                        InflictMsg = ele.ElementInnerText();
                    }
                    break;

                    case "statusMsg":
                    {
                        StatusMsg = ele.ElementInnerText();
                    }
                    break;

                    case "endMsg":
                    {
                        EndMsg = ele.ElementInnerText();
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

        public StatusEffect(StatusEffect sa)
        {
            this.inflictsDamage = sa.inflictsDamage;
            this.MinDamagePercentage = sa.MinDamagePercentage;
            this.MaxDamagePercentage = sa.MaxDamagePercentage;

            this.skipsTurn = sa.skipsTurn;

            this.Controller = sa.Controller;

            this.Explodes = sa.Explodes;
            this.Explosion = new Explosion(sa.Explosion);

            this.Modifiers = sa.Modifiers;

            this.Immunities = sa.Immunities;

            this.Name = sa.Name;
            this.Turns = sa.Turns;
            this.InflictMsg = sa.InflictMsg;
            this.StatusMsg = sa.StatusMsg;
            this.EndMsg = sa.EndMsg;
        }

        public XElement Save()
        {
            XElement ailment = new XElement("ailment",
                new XAttribute("name", Name),
                new XAttribute("controller", Controller),
                new XAttribute("turns", Turns)
                );

            return ailment;
        }

        /// <summary>
        /// Goes through a cycle of the given status effect.
        /// </summary>
        /// <param name="ent">The entity this effect is being applied to.</param>
        /// <param name="tar">The entity the user was targeting or targeted by when the effect was applied.</param>
        /// <param name="attackTeam">Boolean stating whether the entity is on the attacking team or not.</param>
        /// <param name="strife">The strife object itself.</param>
        /// <returns>A tuple containing whether the effect is removed this turn and whether the player's turn is skipped.</returns>
        public Tuple<bool, bool> Update(Entity ent, Entity tar, bool attackTeam, Strife strife)
        {
            if (inflictsDamage)
            {
                // Pick a value to inflict.
                float per = Toolbox.RandFloat(MinDamagePercentage, MaxDamagePercentage);
                int dmg = ent.InflictDamageByPercentage(per);

                strife.Log.AppendLine(Entity.GetEntityMessage(StatusMsg, Syntax.ToCodeLine(ent.Name), Syntax.ToCodeLine(dmg.ToString()), Syntax.ToCodeLine(this.Name)));
            }

            if (skipsTurn)
            {
                if (!string.IsNullOrWhiteSpace(StatusMsg))
                {
                    strife.Log.AppendLine(Entity.GetEntityMessage(StatusMsg, Syntax.ToCodeLine(ent.Name), "{1}", Syntax.ToCodeLine(this.Name)));
                }
            }

            if (Turns == 0 && Explodes)
            {
                strife.Log.AppendLine(strife.Explosion(StatusMsg, Explosion, ent, tar, attackTeam, strife, EXPLOSION_FALLOFF_FACTOR));
            }

            --Turns;
            bool endEffect = Turns < 0;
            strife.Log.AppendLine((endEffect ? "\n" + Entity.GetEntityMessage(EndMsg, Syntax.ToCodeLine(ent.Name), Syntax.ToCodeLine(this.Name)) : string.Empty));

            return new Tuple<bool, bool>
                (
                endEffect,
                skipsTurn
                );
        }

        /// <summary>
        /// Removes the status effect from the specified entity.
        /// </summary>
        /// <returns>The log of the event.</returns>
        public static string RemoveStatusEffect(Entity ent, string name)
        {
            StatusEffect sa = ent.InflictedAilments.FirstOrDefault(x => x.Name.ToLowerInvariant() == name.ToLowerInvariant());

            if (sa == null)
            {
                return string.Empty;
            }

            string msg = sa.EndMsg;
            ent.InflictedAilments.RemoveAll(x => x.Name.ToLowerInvariant() == name.ToLowerInvariant());

            return msg;
        }

        public static bool TryParse(string name, out StatusEffect sa)
        {
            if (Toolbox.StatusEffects.TryGetValue(name, out StatusEffect ail))
            {
                sa = new StatusEffect(ail);
                return true;
            }

            Console.WriteLine("STRIFE ERROR: Ailment \"" + name + "\" not found!");
            sa = null;
            return false;
        }


        public static bool TryParse(string name, out StatusEffect sa, ulong controller, int turns)
        {
            if (TryParse(name, out sa))
            {
                sa.Controller = controller;
                sa.Turns = turns;

                return true;
            }

            return false;
        }
    }
}
