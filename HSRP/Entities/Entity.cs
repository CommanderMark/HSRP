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
        public virtual Item EquippedWeapon { get; }
        public virtual int DiceRolls { get; }

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