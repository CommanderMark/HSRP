using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HSRP
{
    // TODO: Read and write from an xml element functions.
    /// <summary>
    /// Character entity with more variables specific to individual strifes.
    /// </summary>
    public class StrifePlayer : Player, IStrifeEntity
    {
        /// <summary>
        /// Buffs or debuffs applied to stats that remain until the end of the strife.
        /// </summary>
        public AbilitySet Modifiers { get; set; }

        /// <summary>
        /// Buffs or debuffs applied to stats that remain for a specified number of turns.
        /// The key is the number of turns left until the modifier is removed.
        /// </summary>
        public Dictionary<int, AbilitySet> TempMods { get; set; }

        /// <summary>
        /// An AbilitySet containing both the character's base ability stats and their modifiers.
        /// </summary>
        public override AbilitySet Abilities
        {
            get
            {
                AbilitySet aSet = base.Abilities + Modifiers;
                if (TempMods.Any())
                {
                    foreach (KeyValuePair<int, AbilitySet> set in TempMods)
                    {
                        aSet += set.Value;
                    }
                }

                return aSet;
            }

            set
            {
                base.Abilities = value;
            }
        }
    }
}