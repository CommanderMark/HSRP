using System.Collections.Generic;

namespace HSRP
{
    // When you don't feel like using abstract classes you get this.
    static class EntityUtil
    {
        /// <summary>
        /// Inflicts a specified amount of damage on the entity. Input negative values for healing.
        /// </summary>
        /// <returns>Whether the entity's health is below zero after the damage inflicted.</returns>
        public static bool InflictDamage(this IEntity ent, int amount)
        {
            ent.Health -= amount;
            return ent.Health <= 0;
        }

        /// <summary>
        /// Adds a temporary modifier to the entity. If a modifier with that number of turns already exists, add the new modifier to the existing one.
        /// </summary>
        /// <param name="mod">The modifier.</param>
        /// <param name="turnsUntilRemoval">The number of turns that will pass under the modifier is removed.</param>
        public static void AddTempMod(this IEntity ent, AbilitySet mod, int turnsUntilRemoval)
        {
            if (ent.TempMods.ContainsKey(turnsUntilRemoval))
            {
                ent.TempMods[turnsUntilRemoval] += mod;
            }
            else
            {
                ent.TempMods.Add(turnsUntilRemoval, mod);
            }
        }

        /// <summary>
        /// Updates the turn count for every temporary modifier. Removes the modifier if there are 0 turns remaining.
        /// </summary>
        public static void UpdateTempMods(this IEntity ent)
        {
            // If 0 turns left remove 
            if (ent.TempMods.ContainsKey(0))
            {
                ent.TempMods.Remove(0);
            }

            // De-increment the rest.
            foreach (KeyValuePair<int, AbilitySet> mod in ent.TempMods)
            {
                if (mod.Key > 0)
                {
                    ent.TempMods[mod.Key - 1] = mod.Value;
                }
            }
        }
    }
}
