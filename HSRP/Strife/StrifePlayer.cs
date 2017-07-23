namespace HSRP
{
    /// <summary>
    /// Character entity with more variables specific to individual strifes.
    /// </summary>
    public class StrifePlayer : Player
    {
        public AbilitySet Modifiers;

        /// <summary>
        /// An AbilitySet containing both the character's abilities and their modifiers.
        /// </summary>
        public AbilitySet TotalSet
        {
            get
            {
                return base.Abilities + Modifiers;
            }
        }
    }
}