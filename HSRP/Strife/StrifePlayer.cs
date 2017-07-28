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
        /// The key is the number of turns left until the modifier is removed.
        /// </summary>
        private Dictionary<int, AbilitySet> TempMods;

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

        public void AddTempMod(AbilitySet mod, int turnsUntilRemoval)
        {
            // TODO
            if (TempMods.ContainsKey(turnsUntilRemoval))
            {
                TempMods[turnsUntilRemoval] += mod;
            }
            else
            {
                TempMods.Add(turnsUntilRemoval, mod);
            }
        }
    }
}