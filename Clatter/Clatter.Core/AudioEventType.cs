namespace Clatter.Core
{
    /// <summary>
    /// The type of collision audio event.
    /// </summary>
    public enum AudioEventType : byte
    {
        /// <summary>
        /// Corresponds to an Impact event.
        /// </summary>
        impact = 0,
        /// <summary>
        /// Corresponds to a Scrape event.
        /// </summary>
        scrape = 1,
        /// <summary>
        /// Not yet implemented.
        /// </summary>
        roll = 2,
        /// <summary>
        /// A non-event.
        /// </summary>
        none = 3
    }
}