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
            // TODO
            if (ent.TempMods.ContainsKey(turnsUntilRemoval))
            {
                ent.TempMods[turnsUntilRemoval] += mod;
            }
            else
            {
                ent.TempMods.Add(turnsUntilRemoval, mod);
            }
        }

        public static void AddTempMod(this IStrifeEntity ent, string stat, int value, int turnsUntilRemoval)
        {
            AbilitySet set = new AbilitySet();
            foreach (PropertyInfo prop in set.GetType().GetProperties())
            {
                if (stat.ToLower() == prop.Name.ToLower())
                {
                    prop.SetValue(set, value);
                    ent.AddTempMod(set, turnsUntilRemoval);
                    return;
                }
            }
        }
    }
}