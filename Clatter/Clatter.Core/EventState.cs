namespace Clatter.Core
{
    /// <summary>
    /// The current state of an audio event.
    /// </summary>
    public enum EventState : byte
    {
        /// <summary>
        /// The event started on this frame.
        /// </summary>
        start = 0,
        /// <summary>
        /// The event is ongoing.
        /// </summary>
        ongoing = 1,
        /// <summary>
        /// The event ended on this frame.
        /// </summary>
        end = 2
    }
}