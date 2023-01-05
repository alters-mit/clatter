namespace Clatter.Core
{
    /// <summary>
    /// The type of collision audio event.
    /// </summary>
    public enum AudioEventType : byte
    {
        impact = 0,
        scrape = 1,
        roll = 2,
        none = 3
    }
}