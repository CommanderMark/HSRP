using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace HSRP
{
    // TODO: Add thing for preventing moves.
    // TODO: Burning, Bleeding, Stunned
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
        private string description = string.Empty;

        /// <summary>
        /// Whether not the status effect can be applied multiple times.
        /// </summary>
        public bool Stacks = false;

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
            Stacks = element.GetAttributeBool("stacks", false);
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

                    case "description":
                    {
                        description = ele.ElementInnerText();
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

        public StatusEffect() { }
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
            this.description = sa.description;
            this.Stacks = sa.Stacks;
        }

        public XElement Save()
        {
            XElement ailment = new XElement("ailment",
                new XAttribute("name", Name),
                new XAttribute("controller", Controller),
                new XAttribute("turns", Turns)
                );
            
            if (!Modifiers.Equals(new AbilitySet()))
            {
                ailment.Add(Modifiers.ToXmlWithoutEmpties());
            }

            return ailment;
        }

        public string Display()
        {
            StringBuilder result = new StringBuilder();

            result.AppendLine(Syntax.ToBold(Name) + ":");
            if (!string.IsNullOrWhiteSpace(description))
            {
                result.AppendLine(Syntax.ToBold("Description") + ": " + description);
            }

            result.AppendLine();
            if (Turns > 0)
            {
                result.AppendLine($"Lasts {Turns} turns.");
            }
            else
            {
                result.AppendLine($"This status effect is applied immediately instead of over multiple turns.");
            }

            result.AppendLine();
            if (inflictsDamage)
            {
                result.AppendLine($"Does between {MinDamagePercentage * 100}-{MaxDamagePercentage * 100}% of the user's max health in damage each turn.");
            }
            if (skipsTurn)
            {
                result.AppendLine("Skips the user's turn.");
            }
            if (Controller != 0)
            {
                result.AppendLine("Whoever inflicts the status effect will have control over the strifer's mind.");
            }
            if (Explodes)
            {
                result.Append("Causes an explosion. ");
                switch (Explosion.Target)
                {
                    case TargetType.Self:
                    {
                        result.AppendLine("Only affects the user themself.");
                    }
                    break;

                    case TargetType.Target:
                    {
                        result.AppendLine("Only affects the user's target.");
                    }
                    break;

                    case TargetType.Self | TargetType.Target:
                    {
                        result.AppendLine("Only affects the user and their target.");
                    }
                    break;

                    case TargetType.All | TargetType.Self:
                    {
                        result.AppendLine("Only affects the user's target's team.");
                    }
                    break;

                    case TargetType.All | TargetType.Target:
                    {
                        result.AppendLine("Only affects the user's team.");
                    }
                    break;

                    case TargetType.All:
                    {
                        result.AppendLine("Affects every entity in the strife.");
                    }
                    break;
                }
            }

            if (Immunities.Any())
            {
                result.Append("Cures you of " + Immunities[0]);
                for (int i = 1; i < Immunities.Count(); i++)
                {
                    result.Append(", " + Immunities[i]);
                }
                result.AppendLine(".");
            }

            return result.ToString();
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
            int dmg = 0;
            if (inflictsDamage)
            {
                // Pick a value to inflict.
                float per = Toolbox.RandFloat(MinDamagePercentage, MaxDamagePercentage);
                dmg = ent.InflictDamageByPercentage(per);
            }

            if (!string.IsNullOrWhiteSpace(StatusMsg))
            {
                strife.Log.AppendLine(Entity.GetEntityMessage(StatusMsg, Syntax.ToCodeLine(ent.Name), Syntax.ToCodeLine(dmg.ToString()), Syntax.ToCodeLine(this.Name)));
            }

            if (Turns < 1 && Explodes)
            {
                strife.Explosion(Explosion, ent, tar, attackTeam, EXPLOSION_FALLOFF_FACTOR);
            }

            --Turns;
            bool endEffect = Turns < 1;

            return new Tuple<bool, bool>
                (
                endEffect,
                skipsTurn
                );
        }

        public static bool TryParse(string name, out StatusEffect sa)
        {
            if (Toolbox.StatusEffects.TryGetValue(name, out StatusEffect ail))
            {
                sa = new StatusEffect(ail);
                return true;
            }
            
            sa = null;
            return false;
        }
    }
}
