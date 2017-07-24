using System.Collections.Generic;
using System.Linq;

namespace HSRP
{
    /// <summary>
    /// Character entity with more variables specific to individual strifes.
    /// </summary>
    public class StrifePlayer : Player
    {
        /// <summary>
        /// Buffs or debuffs applied to stats that remain until the end of the strife.
        /// </summary>
        public AbilitySet Modifiers;

        /// <summary>
        /// Buffs or debuffs applied to stats that remain for a specified number of turns.
        /// The index of the list corresponds to the number of turns left until the modifier is removed.
        /// </summary>
        public List<List<AbilitySet>> TempMods;

        /// <summary>
        /// An AbilitySet containing both the character's abilitie stats and their modifiers.
        /// </summary>
        public AbilitySet TotalAbilityStats
        {
            get
            {
                AbilitySet aSet = base.Abilities + Modifiers;
                if (TempMods.Any())
                {
                    foreach (List<AbilitySet> abList in TempMods)
                    {
                        abList.ForEach(set => aSet += set);
                    }
                }

                return aSet;
            }
        }
    }
}