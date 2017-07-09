using System;
using System.Collections.Generic;
using System.Text;

namespace HSRP
{
    public class Player
    {
        public AbilitySet Abilities { get; set; }

        public int Will { get; set; }

        #region Active Stats

        public int Health { get; set; }
        public int Armor { get; set; }
        
        public string Specibus { get; set; }

        public bool Psion { get; private set; }
        // Only use if Psion is true.
        public int PowerPoints { get; set; }

        #endregion
    }
}
