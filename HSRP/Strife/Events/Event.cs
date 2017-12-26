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
        /// <param name="ent">The entity this handler is attached to.</param>
        /// <param name="tar">The entity the user was targeting or targeted by when the handler was triggered.</param>
        /// <param name="strife">The strife object itself.</param>
        /// <returns>The log of the event.</returns>
        public delegate string Handler(IEntity ent, IEntity tar, Strife strife);

        /// <summary>
        /// The probability of the event firing when the condition is met.
        /// Default is 1 (100%).
        /// </summary>
        public int Probability = 1;
    }
}
