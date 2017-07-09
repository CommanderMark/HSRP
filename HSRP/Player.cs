using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HSRP
{
    public class Player
    {
        public ulong ID { get; set; }
        public string Name { get; set; }

        public AbilitySet Abilities { get; set; }

        public int Health { get; set; }
        public int Armor { get; set; }
        public string Specibus { get; set; }

        public int Echeladder { get; private set; }
        public int PendingLevelUps { get; set; }
        public int PendingSkillPointAllocations { get; set; }

        // Stats based on previously mentioned variables.
        public int Will
        {
            get
            {
                return 10 + Abilities.Wisdom + Abilities.Charisma;
            }
        }

        public int Persuation
        {
            get
            {
                return Abilities.Intelligence + Abilities.Charisma;
            }
        }

        public int Intimidation
        {
            get
            {
                return Abilities.Strength - Abilities.Charisma;
            }
        }

        public Player(ulong ID) : this(ID.ToString()) { }
        public Player(string filePath)
        {
            string path = filePath.Contains(Dirs.Players)
                ? filePath + ".xml"
                : Path.Combine(Dirs.Players, filePath) + ".xml";
        }

        /// <summary>
        /// Levels up your character.
        /// </summary>
        /// <returns>Whether or not you're due for a skill point increase.</returns>
        public bool LevelUp()
        {
            Echeladder++;
            PendingLevelUps--;

            Health += Toolbox.DiceRoll(1, 6 + Abilities.Constitution);

            if (Echeladder % 2 == 0)
            {
                PendingSkillPointAllocations++;
                return true;
            }
            return false;
        }
    }
}
