using System;
using System.Collections.Generic;
using System.Text;

namespace HSRP
{
    /// <summary>
    /// A class for handing event triggers in a strife.
    /// </summary>
    public class Events
    {
        /// <summary>
        /// Event triggers when the calling entity strikes a hit on another entity.
        /// </summary>
        public Event OnHit;

        /// <summary>
        /// Event triggers when the calling entity is attacked by another entity.
        /// </summary>
        public Event OnAttacked;

        /// <summary>
        /// Event triggers when the calling entity kills another.
        /// </summary>
        public Event OnKill;

        /// <summary>
        /// Event triggers when the calling entity dies.
        /// </summary>
        public Event OnDeath;

        /// <summary>
        /// Event triggers when the calling entity .
        /// </summary>
        public Dictionary<string, Event> OnStatusEffect;

        public
    }
}
