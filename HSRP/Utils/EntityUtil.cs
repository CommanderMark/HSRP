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
    }
}
