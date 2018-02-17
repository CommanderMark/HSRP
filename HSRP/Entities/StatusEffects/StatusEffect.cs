using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace HSRP
{
    // TODO: Add thing for preventing moves.
    // TODO: Add check for preventing moves to NPCs as well.
    // TODO: Burning, Bleeding
    public class StatusEffect
    {
        // Inflict damage stuff.
        private InflictDamage damage;

        // Skip a turn.
        private bool skipsTurn;

        // Mind control.
        public ulong Controller;

        /// <summary>
        /// Makes the inflicted immune to damage.
        /// </summary>
        public bool DamageImmune;

        /// <summary>
        /// Prevents the inflicted from using moves.
        /// </summary>
        public bool BlocksMoves;

        // Explosion
        public bool Explodes;
        public Explosion Explosion;

        // Stat buffs/debuffs.
        public AbilitySet Modifiers;

        public string[] Immunities;

        // Misc. general status effect stuff.
        public string Name;
        public int Turns;
        public string InflictMsg;
        public string StatusMsg;
        public string EndMsg;
        private string description;

        /// <summary>
        /// Whether not the status effect can be applied multiple times.
        /// </summary>
        public bool Stacks;

        public const int EXPLOSION_FALLOFF_FACTOR = 2;

        public StatusEffect()
        {
            damage = new InflictDamage();
            skipsTurn = false;
            Controller = 0;
            DamageImmune = false;
            BlocksMoves = false;
            Explodes = false;
            Explosion = new Explosion();
            Modifiers = new AbilitySet();
            Immunities = new string[0];

            Name = string.Empty;
            Turns = 0;
            InflictMsg = string.Empty;
            StatusMsg = string.Empty;
            EndMsg = string.Empty;
            description = string.Empty;

            Stacks = false;
        }

        public StatusEffect(XElement element) : this()
        {
            Name = element.GetAttributeString("name", string.Empty);
            
            // If the element has no sub-elements then try to parse them from the master list.
            if (!element.HasElements)
            {
                TryParse(Name, this);
            }

            // Pull any different attributes from the element itself.
            skipsTurn = element.GetAttributeBool("skipTurns", this.skipsTurn);
            Controller = XmlToolbox.GetAttributeUnsignedLong(element, "controller", this.Controller);
            DamageImmune = XmlToolbox.GetAttributeBool(element, "damageImmune", this.DamageImmune);
            BlocksMoves = XmlToolbox.GetAttributeBool(element, "blocksMoves", this.BlocksMoves);
            Controller = XmlToolbox.GetAttributeUnsignedLong(element, "controller", this.Controller);
            Explodes = element.GetAttributeBool("explodes", this.Explodes);
            Turns = element.GetAttributeInt("turns", this.Turns);
            Stacks = element.GetAttributeBool("stacks", this.Stacks);
            Immunities = XmlToolbox.GetAttributeStringArray(element, "immune", new string[0]);

            foreach (XElement ele in element.Elements())
            {
                switch (ele.Name.LocalName)
                {
                    case "inflictDamage":
                    {
                        damage = new InflictDamage(ele);
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

        public StatusEffect(StatusEffect sa)
        {
            this.Copy(sa);
        }

        private void Copy(StatusEffect copy)
        {
            this.damage = new InflictDamage(copy.damage);

            this.skipsTurn = copy.skipsTurn;

            this.Controller = copy.Controller;

            this.Explodes = copy.Explodes;
            this.Explosion = new Explosion(copy.Explosion);

            this.Modifiers = new AbilitySet(copy.Modifiers);

            this.Immunities = copy.Immunities;

            this.Name = copy.Name;
            this.Turns = copy.Turns;
            this.InflictMsg = copy.InflictMsg;
            this.StatusMsg = copy.StatusMsg;
            this.EndMsg = copy.EndMsg;
            this.description = copy.description;
            this.Stacks = copy.Stacks;
        }

        public XElement Save()
        {
            XElement ailment = new XElement("ailment",
                new XAttribute("name", Name)
                );
            
            if (Controller != 0)
            {
                ailment.Add(new XAttribute("controller", Controller));
            }
            if (DamageImmune)
            {
                ailment.Add(new XAttribute("damageImmune", DamageImmune));
            }
            if (BlocksMoves)
            {
                ailment.Add(new XAttribute("blocksMoves", BlocksMoves));
            }
            ailment.Add(new XAttribute("turns", Turns));

            if (Immunities.Count() > 0)
            {
                ailment.Add(new XAttribute("immune", string.Join(",", Immunities)));
            }
            
            if (!Modifiers.Equals(new AbilitySet()))
            {
                ailment.Add(Modifiers.ToXmlWithoutEmpties());
            }

            return ailment;
        }

        public XElement SaveAllData()
        {
            XElement ailment = this.Save();
            
            if (damage.MinAmount >= InflictDamage.MarginOfError
                || damage.MaxAmount >= InflictDamage.MarginOfError)
            {
                ailment.Add(damage.Save());
            }
            
            if (Explodes)
            {
                ailment.Add(new XAttribute("explodes", true));
                ailment.Add(Explosion.Save());
            }

            if (!string.IsNullOrWhiteSpace(InflictMsg))
            {
                ailment.Add(new XElement("inflictMsg", new XText(InflictMsg)));
            }

            if (!string.IsNullOrWhiteSpace(StatusMsg))
            {
                ailment.Add(new XElement("statusMsg", new XText(StatusMsg)));
            }

            if (!string.IsNullOrWhiteSpace(EndMsg))
            {
                ailment.Add(new XElement("endMsg", new XText(EndMsg)));
            }

            if (!string.IsNullOrWhiteSpace(description))
            {
                ailment.Add(new XElement("description", new XText(description)));
            }

            return ailment;
        }

        // TODO:
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
            if (damage.HasDamage)
            {
                if (!damage.FixedAmount)
                {
                    string msg = damage.Ranged
                        ? $"between {damage.MinAmount * 100}-{damage.MaxAmount * 100}"
                        : (damage.MinAmount * 100).ToString();
                    result.AppendLine($"Does {msg}% of the user's max health in damage each turn.");
                }
                else
                {
                    string msg = damage.Ranged
                        ? $"between {damage.MinAmount}-{damage.MaxAmount}"
                        : damage.MinAmount.ToString();
                    result.AppendLine($"Does {msg} damage each turn.");
                }
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
        /// <returns>Whether the player's turn is skipped.</returns>
        public bool Update(Entity ent, Entity tar, bool attackTeam, Strife strife)
        {

            if (!string.IsNullOrWhiteSpace(StatusMsg))
            {
                strife.Log.AppendLine(Entity.GetEntityMessage(StatusMsg, Syntax.ToCodeLine(ent.Name), Syntax.ToCodeLine(this.Name)));
            }

            if (damage.HasDamage)
            {
                damage.ApplyDamage(ent, strife);
            }

            if (Turns < 1 && Explodes)
            {
                strife.Explosion(Explosion, ent, tar, attackTeam, EXPLOSION_FALLOFF_FACTOR);
            }

            --Turns;
            bool endEffect = Turns < 1;

            return skipsTurn;
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

        public static bool TryParse(string name, StatusEffect sa)
        {
            if (Toolbox.StatusEffects.TryGetValue(sa.Name, out StatusEffect ail))
            {
                sa.Copy(ail);
                return true;
            }
            
            return false;
        }
    }
}
