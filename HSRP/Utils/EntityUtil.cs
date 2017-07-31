namespace HSRP
{
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
    }
}
