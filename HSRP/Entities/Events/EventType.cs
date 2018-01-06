namespace HSRP
{
    /// <summary>
    /// A class for handing types of event triggers in a strife.
    /// </summary>
    public enum EventType
    {
        NONE,

        /// <summary>
        /// Event triggers when the calling entity strikes a hit on another entity.
        /// </summary>
        OnHit,

        /// <summary>
        /// Event triggers when the calling entity is attacked by another entity.
        /// </summary>
        OnAttacked,

        /// <summary>
        /// Event triggers when the calling entity kills another.
        /// </summary>
        OnKill,

        /// <summary>
        /// Event triggers when the calling entity dies.
        /// </summary>
        OnDeath
    }
}
