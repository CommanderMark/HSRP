using System;
using System.Collections.Generic;
using System.Linq;

namespace HSRP
{
    // When you don't feel like using abstract classes you get this.
    static class EntityUtil
    {
        /// <summary>
        /// Inflicts a specified amount of damage on the entity. Input negative values for healing.
        /// </summary>
        /// <returns>Whether the entity's health is below or equal to zero after the damage inflicted.</returns>
        public static bool InflictDamage(this IEntity ent, int amount)
        {
            ent.Health -= amount;
            return ent.Health <= 0;
        }

        /// <summary>
        /// Inflicts a specified amount of damage on the entity.
        /// Amount of damage is based on a percentage multiplied by their max health to calculate damage.
        /// </summary>
        public static int InflictDamageByPercentage(this IEntity ent, float percentage)
        {
            int damage = (int)Math.Round(ent.MaxHealth * percentage, MidpointRounding.AwayFromZero);
            ent.Health -= damage;

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

        public static AbilitySet GetModifiers(this IEntity ent)
        {
            AbilitySet uh = new AbilitySet();
            foreach (StatusEffect sa in ent.InflictedAilments)
            {
                uh += sa.Modifiers;
            }

            return uh;
        }

        public static AbilitySet GetTotalAbilities(this IEntity ent)
        {
            return ent.BaseAbilities + ent.GetModifiers();
        }

        public static ulong GetMindController(this IEntity ent)
        {
            foreach (StatusEffect sa in ent.InflictedAilments)
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
