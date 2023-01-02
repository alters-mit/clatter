namespace Clatter.Core
{
    /// <summary>
    /// A CollisionEvent derives audio data needed for audio events from a collision. The type and nature of the collision will determine whether the collision generates impact audio, scrape audio, or neither.
    ///
    /// Collisions always have two `AudioObjectData` objects: A primary (usually faster-moving) object, and a secondary (slower-moving or stationary) object. For scrape events, the secondary object must have a `ScrapeMaterial`.
    /// </summary>
    public readonly struct CollisionEvent
    {
        /// <summary>
        /// The primary object.
        /// </summary>
        public readonly AudioObjectData primary;
        /// <summary>
        /// The secondary object.
        /// </summary>
        public readonly AudioObjectData secondary;
        /// <summary>
        /// The combined object IDs pair as a long. This is used by `AudioGenerator` as a dictionary key.
        /// </summary>
        public readonly ulong ids;
        /// <summary>
        /// The type of the audio event (impact, scrape, roll, none).
        /// </summary>
        public readonly AudioEventType type;
        /// <summary>
        /// The centroid of the collision.
        /// </summary>
        public readonly Vector3d centroid;
        /// <summary>
        /// The speed of the collision.
        /// </summary>
        public readonly double speed;
        /// <summary>
        /// The area of the collision.
        /// </summary>
        public readonly double area;
        
        
        /// <summary>
        /// (constructor)
        /// </summary>
        /// <param name="primary">The primary object.</param>
        /// <param name="secondary">The secondary object.</param>
        /// <param name="type">The type of the audio event (impact, scrape, roll, none).</param>
        /// <param name="speed">The speed of the collision.</param>
        /// <param name="area">The area of the collision.</param>
        /// <param name="centroid">The centroid of the collision.</param>
        public CollisionEvent(AudioObjectData primary, AudioObjectData secondary, AudioEventType type, double speed, double area, Vector3d centroid)
        {
            this.primary = primary;
            this.secondary = secondary;
            this.type = type;
            this.speed = speed;
            this.area = area;
            this.centroid = centroid;
            // Get an object ID pair as a long. Source: https://stackoverflow.com/a/827267
            ids = ((ulong)primary.id << 32) | secondary.id;
        }
    }
}