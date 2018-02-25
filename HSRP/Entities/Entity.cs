using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HSRP
{
    public abstract class Entity : IEventable
    {
        public ulong ID { get; set; }
        public virtual string Name { get; set; }

        public bool LikesPineappleOnPizza { get; set; }

        public AbilitySet BaseAbilities { get; set; }

        public List<Tuple<EventType, Event>> Events { get; set; }
        public string[] Immunities { get; set; }
        public List<StatusEffect> InflictedAilments { get; set; }
        public Dictionary<string, Move> Moves { get; set; }

        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public bool Dead { get; set; }

        public string Specibus { get; set; }
        public virtual int DiceRolls { set; get; }

        protected Entity()
        {
            BaseAbilities = new AbilitySet();

            Events = new List<Tuple<EventType, Event>>();
            Immunities = null;
            InflictedAilments = new List<StatusEffect>();
            Moves = new Dictionary<string, Move>();

            Name = "";
            Specibus = "";

            Events.Add(new Tuple<EventType, Event>(EventType.OnAttacked, Event.WakeUpAfterHit));
        }

        public abstract string Display(bool showMods);

        public string DisplayAilmentsAndMoves()
        {
            StringBuilder msg = new StringBuilder();
            string separator = "--";

            // Moves.
            msg.AppendLine(Syntax.ToCodeLine("Status Effects:"));
            IEnumerable<StatusEffect> displayList = from x in InflictedAilments
                                    where x.Name != Constants.PHYSICAL_COUNTER_AIL && x.Name != Constants.SPE_ATTACK_AIL
                                    select x;
            if (displayList.Any())
            {
                foreach (StatusEffect sa in displayList)
                {
                    msg.Append(sa.Display());
                }
            }
            else
            {
                msg.AppendLine("None");
            }

            msg.AppendLine();
            msg.AppendLine(separator);
            msg.AppendLine();

            // Events.
            msg.AppendLine(Syntax.ToCodeLine("Moves:"));
            if (Moves.Values.Any())
            {
                foreach (Move attack in Moves.Values)
                {
                    msg.Append(Syntax.ToBold(attack.Name) + ": ");
                    msg.AppendLine(attack.Description);
                    
                    msg.AppendLine(separator);
                    msg.AppendLine();
                }
            }
            else
            {
                msg.Append("None");
            }

            return msg.ToString();
        }

        // <summary>
        /// Inflicts a specified amount of damage on the entity. Input negative values for healing.
        /// </summary>
        /// <returns>Whether the entity's health is below or equal to zero after the damage inflicted.</returns>
        public bool InflictDamage(int amount)
        {
            this.Health -= amount;
            return this.Health <= 0;
        }

        public void HealDamageByPercentage(float percentage, Strife strife)
        {
            int damage = (int) Math.Round(this.MaxHealth * percentage, MidpointRounding.AwayFromZero);
            this.Health += damage;

            strife.Log.AppendLine($"{Syntax.ToCodeLine(this.Name)} was healed for {Syntax.ToCodeLine(damage)} health.");
        }

        /// <summary>
        /// Applies a status effect to an entity.
        /// </summary>
        /// <param name="ailName">Name of the effect.</param>
        /// <param name="tar">The entity the user was targeting or targeted by when the effect was applied.</param>
        /// <param name="attackTeam">Boolean stating whether the entity is on the attacking team or not.</param>
        /// <param name="strife">The strife object itself.</param>
        public void ApplyStatusEffect(string ailName, Entity tar, bool attackTeam, Strife strife)
        {
            if (!StatusEffect.TryParse(ailName, out StatusEffect sa))
            {
                return;
            }
            ApplyStatusEffect(sa, tar, attackTeam, strife);
        }

        /// <summary>
        /// Applies a status effect to an entity.
        /// </summary>
        /// <param name="sa">The status effect.</param>
        /// <param name="tar">The entity the user was targeting or targeted by when the effect was applied.</param>
        /// <param name="attackTeam">Boolean stating whether the entity is on the attacking team or not.</param>
        /// <param name="strife">The strife object itself.</param>
        public void ApplyStatusEffect(StatusEffect sa, Entity tar, bool attackTeam, Strife strife)
        {
            // Are they immune?
            foreach (string name in this.Immunities)
            {
                if (sa.Name.ToLowerInvariant() == name.ToLowerInvariant())
                {
                    strife.Log.AppendLine($"{Syntax.ToCodeLine(this.Name)} is immune to \"{Syntax.ToCodeLine(name.ToString())}\"!");
                    return;
                }
            }

            // Do they already have this status effect or posses one which makes them immune?
            if (!sa.Stacks)
            {
                foreach (StatusEffect saOther in this.InflictedAilments)
                {
                    if (sa.Name.ToLowerInvariant() == saOther.Name.ToLowerInvariant() && saOther.Turns > 0)
                    {
                        strife.Log.AppendLine($"{Syntax.ToCodeLine(this.Name)} is already \"{Syntax.ToCodeLine(sa.Name.ToString())}\"!");
                        return;
                    }
                    foreach (string name in saOther.Immunities)
                    {
                        if (sa.Name.ToLowerInvariant() == name.ToLowerInvariant())
                        {
                            strife.Log.AppendLine($"{Syntax.ToCodeLine(this.Name)} is immune to \"{Syntax.ToCodeLine(name.ToString())}\"!");
                            return;
                        }
                    }
                }
            }

            // If the modifier affects a percentage of health convert it to a fixed number.
            sa.Modifiers = AbilitySet.ToFixedNumber(sa.Modifiers, this.BaseAbilities);

            // Check that it is not an immediate status effect.
            if (sa.Turns < 1)
            {
                sa.Update(this, tar, attackTeam, strife);
                return;
            }

            if (sa.Controller != 0)
            {
                sa.Controller = tar.ID;
            }

            this.InflictedAilments.Add(sa);

            if (!string.IsNullOrWhiteSpace(sa.InflictMsg))
            {
                strife.Log.AppendLine(GetEntityMessage(sa.InflictMsg, Syntax.ToCodeLine(this.Name), Syntax.ToCodeLine(tar.Name), Syntax.ToCodeLine(sa.Name)));
            }
        }

        /// <summary>
        /// Removes the status effect from the specified entity.
        /// </summary>
        /// <param name="name">Name of the status effect.</param>
        /// <param name="strife">The strife object itself.</param>
        /// <param name="turnSensitive">When set to true, only removes the status effect if it's turn counter is below 0.
        /// I.E. if a status effect can be applied multiple times at once to an entity then this would be set to true
        /// so as to make sure this command only removes the iteration of it who's turn count is below 1.</param>
        public void RemoveStatusEffect(string name, Strife strife, bool turnSensitive)
        {
            StatusEffect sa = InflictedAilments.FirstOrDefault(x => x.Name.ToLowerInvariant() == name.ToLowerInvariant());
            if (!string.IsNullOrWhiteSpace(sa?.EndMsg))
            {
                strife.Log.AppendLine(GetEntityMessage(sa.EndMsg, Syntax.ToCodeLine(this.Name), Syntax.ToCodeLine(sa.Name)));
            }

            if (!turnSensitive)
            {
                InflictedAilments.RemoveAll(x => x.Name.ToLowerInvariant() == name.ToLowerInvariant());
            }
            else
            {
                InflictedAilments.RemoveAll(x =>
                    x.Name.ToLowerInvariant() == name.ToLowerInvariant()
                    && x.Turns < 1
                    );
            }
        }

        /// <summary>
        /// Triggers an event attached to the entity if one is present.
        /// </summary>
        /// <param name="tar">The entity the user was targeting or targeted by when the event was triggered.</param>
        /// <param name="attackTeam">Boolean stating whether the entity is on the attacking team or not.</param>
        /// <param name="strife">The strife object itself.</param>
        // One thing that could potentially be done in the future is to give status effects these types of triggers
        // and run them here.
        public void TriggerEvent(EventType type, Entity tar, bool attackTeam, Strife strife)
        {
            foreach (Tuple<EventType, Event> tup in Events)
            {
                if (tup.Item1 == type)
                {
                    tup.Item2.Fire(this, tar, attackTeam, strife);
                }
            }
        }

        /// <summary>
        /// Generates an entity-specific message.
        /// </summary>
        public static string GetEntityMessage(string msg, params string[] args)
        {
            if (args != null)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    msg = msg.Replace("{" + i + "}", args[i]);
                }
            }
            
            return msg;
        }

        public AbilitySet GetModifiers()
        {
            AbilitySet uh = new AbilitySet();
            foreach (StatusEffect sa in this.InflictedAilments)
            {
                uh += sa.Modifiers;
            }

            return uh;
        }

        public AbilitySet GetTotalAbilities()
        {
            return this.BaseAbilities + this.GetModifiers();
        }

        public int GetAbilityValue(string ability)
        {
            switch (ability)
            {
                case Ability.STR:
                {
                    return this.GetTotalAbilities().Strength.Value;
                }

                case Ability.CON:
                {
                    return this.GetTotalAbilities().Constitution.Value;
                }

                case Ability.PSI:
                {
                    return this.GetTotalAbilities().Psion.Value;
                }

                case Ability.FOR:
                {
                    return this.GetTotalAbilities().Fortitude.Value;
                }

                case Ability.INT:
                {
                    return this.GetTotalAbilities().Intimidation.Value;
                }

                case Ability.PER:
                {
                    return this.GetTotalAbilities().Persuasion.Value;
                }
            }

            Console.WriteLine("Error: Ability \"" + ability + "\" not found for " + this.Name + ".");
            return 0;
        }

        public ulong GetMindController()
        {
            foreach (StatusEffect sa in this.InflictedAilments)
            {
                if (sa.Controller > 0)
                {
                    return sa.Controller;
                }
            }

            return 0;
        }

        /// <summary>
        /// Returns a boolean indicating whether the entity has a status effect that makes them immune to damage.
        /// </summary>
        public bool ImmuneToDamage()
        {
            foreach (StatusEffect sa in this.InflictedAilments)
            {
                if (sa.DamageImmune)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns a boolean indicating whether the entity has a status effect that prevents move-usage.
        /// </summary>
        public bool BlockedMoves()
        {
            foreach (StatusEffect sa in this.InflictedAilments)
            {
                if (sa.BlocksMoves)
                {
                    return true;
                }
            }

            return false;
        }
    }
}