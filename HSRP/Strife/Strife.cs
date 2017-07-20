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

        public int ID { get; set; }
    }
}
