using System.Collections.Generic;
using System.Reflection;

namespace HSRP
{
    public interface IStrifeEntity
    {
        AbilitySet Modifiers { get; set; }

        Dictionary<int, AbilitySet> TempMods { get; set; }
    }

    public static class StrifeEntityExt
    {
        public static void AddTempMod(this IStrifeEntity ent, AbilitySet mod, int turnsUntilRemoval)
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