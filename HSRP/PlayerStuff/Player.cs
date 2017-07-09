using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace HSRP
{
    public class Player
    {
        public ulong ID { get; set; }
        public string Name { get; set; }

        private AbilitySet abilities { get; set; }
        private AbilitySet abilityModifiers { get; set; }

        /// <summary>
        /// Total ability skill set.
        /// </summary>
        public AbilitySet Abilities 
        {
            get
            {
                return abilities + abilityModifiers;
            }
        }

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

        public bool Errored { get; set; }

        public Player(Discord.IUser user) : this(user.Id.ToString()) { }
        public Player(ulong ID) : this(ID.ToString()) { }
        public Player(string filePath)
        {
            string path = filePath.Contains(Dirs.Players)
                ? filePath + ".xml"
                : Path.Combine(Dirs.Players, filePath) + ".xml";

            XDocument doc = XmlToolbox.TryLoadXml(filePath);
            if (doc == null) { Errored = true; return; }
            foreach (XElement ele in doc.Root.Elements())
            {

            }
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

            if (Math.Log(Echeladder, 2) % 1 == 0)
            {
                PendingSkillPointAllocations++;
                return true;
            }
            return false;
        }

        // Static utils.
        public static bool Registered(ulong plyer)
        {
            return Directory.Exists(Path.Combine(Dirs.Players, plyer.ToString()));
        }
    }
}
