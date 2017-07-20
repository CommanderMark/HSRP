using System;
using System.Collections.Generic;
using System.Text;

namespace HSRP
{
    // Could make this an abstract class for PvE + PvP. So saving methods are abstract but properties
    // and other stuff (oh GAWD the other stuff) is done here.
    public class Strife
    {
        public static LinkedList<Strife> Strifes { get; set; }

        /// <summary>
        /// Whether the Attackers team is taking their turn or not.
        /// </summary>
        public bool AttackTurn { get; set; }
        /// <summary>
        /// Who in the current team's (Attackers or Targets) turn it is. Starts at 0.
        /// </summary>
        public int Turn { get; set; }

        public LinkedList<Player> Attackers { get; set; }
        public LinkedList<NPC> Targets { get; set; }
    }
}
