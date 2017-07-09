using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HSRP
{
    public static class Toolbox
    {
        public static int DiceRoll(int rolls, int dieType = 6)
        {
            int total = 0;
            for (int i = 0; i < rolls; i++)
            {
                total += RandInt(1, dieType, true);
            }

            return total;
        }

        // Random int generator that can output different
        // numbers in the same tick.
        private static readonly Random rand = new Random();
        private static readonly object synclock = new object();
        public static int RandInt(int min, int max, bool inclusive = false)
        {
            // Locking allows different numbers per tick.
            lock (synclock)
            {
                return inclusive
                    ? rand.Next(max - min + 1) + min
                    : rand.Next(min, max);
            }
        }

        public static int RandInt(int max, bool inclusive = false) => RandInt(0, max, inclusive);

        public static void DebugWriteLine(params object[] args)
        {
            foreach (object obj in args)
            {
                Console.WriteLine(obj.ToString());
            }
        }

        public static PropertyInfo GetAbilityProperty(this BaseAbility ability)
        {
            Type type = typeof(AbilitySet);
            foreach (PropertyInfo prop in type.GetProperties())
            {
                if (prop.Name == ability.ToString())
                {
                    return prop;
                }
            }

            return null;
        }
    }
}
