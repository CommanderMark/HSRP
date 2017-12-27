using System.Collections.Generic;

namespace HSRP
{
    /// <summary>
    /// A class for handing event triggers in a strife.
    /// </summary>
    public class Event
    {
        /// <summary>
        /// Contains the effects of the event to invoke on the entity.
        /// </summary>
        private List<StatusEffect> statusEffects;

        /// <summary>
        /// The probability of the event firing when the condition is met.
        /// Default is 1 (100%).
        /// </summary>
        public int Probability = 1;
    }
}
