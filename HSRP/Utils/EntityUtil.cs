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
        /// Applies a status effect to an entity.
        /// </summary>
        /// <param name="ailName">Name of the effect.</param>
        /// <param name="tar">The entity the user was targeting or targeted by when the effect was applied.</param>
        /// <param name="strife">The strife object itself.</param>
        /// <returns>The log of the event.</returns>
        public static string ApplyStatusEffect(this IEntity ent, string ailName, IEntity tar, Strife strife)
        {
            if (!StatusEffect.TryParse(ailName, out StatusEffect sa))
            {
                return string.Empty;
            }

            // Are they immune?
            foreach (string name in ent.Immunities)
            {
                if (sa.Name.ToLowerInvariant() == name.ToLowerInvariant())
                {
                    return $"{Syntax.ToCodeLine(ent.Name)} is immune to \"{Syntax.ToCodeLine(name.ToString())}\"!";
                }
            }

            // Do they already have this status effect or posses one which makes them immune?
            foreach (StatusEffect saOther in ent.InflictedAilments)
            {
                if (sa.Name.ToLowerInvariant() == saOther.Name.ToLowerInvariant())
                {
                    return $"{Syntax.ToCodeLine(ent.Name)} is already \"{Syntax.ToCodeLine(sa.Name.ToString())}\"!";
                }
                foreach (string name in saOther.Immunities)
                {
                    if (sa.Name.ToLowerInvariant() == name.ToLowerInvariant())
                    {
                        return $"{Syntax.ToCodeLine(ent.Name)} is immune to \"{Syntax.ToCodeLine(name.ToString())}\"!";
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

                    return string.Empty;
                }
                else
                {
                    // Boom boom now.
                    return strife.Explosion(sa.InflictMsg, sa.Explosion, ent, tar, StatusEffect.EXPLOSION_FALLOFF_FACTOR);
                }
            }

            ent.InflictedAilments.Add(sa);
            return GetEntityMessage(sa.InflictMsg, Syntax.ToCodeLine(ent.Name), Syntax.ToCodeLine(tar.Name), Syntax.ToCodeLine(sa.Name));
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
