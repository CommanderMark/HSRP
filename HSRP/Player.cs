using System;
using System.Collections.Generic;
using System.Text;

namespace HSRP
{
    public class Player
    {
        #region Ability Modifiers

        public int Strength { get; set; }
        public int Dexterity { get; set; }
        public int Constitution { get; set; }
        public int Intelligence { get; set; }
        public int Wisdom { get; set; }
        public int Charisma { get; set; }

        #endregion


        #region Active Stats

        public int Health { get; set; }
        public int Armor { get; set; }

        #endregion


        #region Passive Stats
        
        public string Specibus { get; set; }
        public bool Psion { get; private set; }
        // Only use if Psion is true.
        public int PowerPoints { get; set; }

        #endregion
    }
}
