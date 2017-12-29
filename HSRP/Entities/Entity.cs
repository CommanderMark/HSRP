using System;
using System.Collections.Generic;

namespace HSRP
{
    public abstract class Entity : IEventable
    {
        public ulong ID { get; set; }
        public virtual string Name { get; set; }

        public bool LikesPineappleOnPizza { get; set; }

        public AbilitySet BaseAbilities { get; set; }

        public Dictionary<EventType, Event> Events { get; set; }
        public string[] Immunities { get; set; }
        public List<StatusEffect> InflictedAilments { get; set; }
        public List<Move> Moves { get; set; }

        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public bool Dead { get; set; }

        public string Specibus { get; set; }
        public virtual Item EquippedWeapon { set; get; }
        public virtual int DiceRolls { set; get; }

        public abstract string Display(bool showMods);

        // <summary>
        /// Inflicts a specified amount of damage on the entity. Input negative values for healing.
        /// </summary>
        /// <returns>Whether the entity's health is below or equal to zero after the damage inflicted.</returns>
        public bool InflictDamage(int amount)
        {
            this.Health -= amount;
            return this.Health <= 0;
        }

        /// <summary>
        /// Inflicts a specified amount of damage on the entity.
        /// Amount of damage is based on a percentage multiplied by their max health to calculate damage.
        /// </summary>
        public int InflictDamageByPercentage(float percentage)
        {
            int damage = (int)Math.Round(this.MaxHealth * percentage, MidpointRounding.AwayFromZero);
            this.Health -= damage;

            return damage;
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
            foreach (StatusEffect saOther in this.InflictedAilments)
            {
                if (sa.Name.ToLowerInvariant() == saOther.Name.ToLowerInvariant())
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

            // Check that it is not an immediate explosion or invalid.
            if (sa.Turns < 0)
            {
                if (!sa.Explodes)
                {
                    Console.WriteLine("STRIFE ERROR: invalid turn count!"
                        + " (Name: " + sa.Name + ", TurnCount: " + sa.Turns + sa.MaxDamagePercentage + ")");

                    return;
                }
                else
                {
                    // Boom boom now.
                    return strife.Explosion(sa.InflictMsg, sa.Explosion, this, tar, attackTeam, strife, StatusEffect.EXPLOSION_FALLOFF_FACTOR);
                }
            }

            this.InflictedAilments.Add(sa);
            if (!string.IsNullOrWhiteSpace(sa.InflictMsg))
            {
                strife.Log.AppendLine(GetEntityMessage(sa.InflictMsg, Syntax.ToCodeLine(this.Name), Syntax.ToCodeLine(tar.Name), Syntax.ToCodeLine(sa.Name)));
            }
        }

        /// <summary>
        /// Triggers an event attached to the entity if one is present.
        /// </summary>
        /// <param name="tar">The entity the user was targeting or targeted by when the event was triggered.</param>
        /// <param name="attackTeam">Boolean stating whether the entity is on the attacking team or not.</param>
        /// <param name="strife">The strife object itself.</param>
        public void TriggerEvent(EventType type, Entity tar, bool attackTeam, Strife strife)
        {
            if (this.Events.TryGetValue(type, out Event evnt))
            {
                evnt.Fire(this, tar, attackTeam, strife);
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

        public ulong GetMindController()
        {
            foreach (StatusEffect sa in this.InflictedAilments)
            {
                if (sa.Controller >= 0)
                {
                    return sa.Controller;
                }
            }

            return 0;
        }
    }
}