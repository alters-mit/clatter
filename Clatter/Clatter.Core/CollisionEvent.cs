namespace Clatter.Core
{
    /// <summary>
    /// A CollisionEvent stores collision data, which will later be converted into audio data.
    ///
    /// Typically, a CollisionEvent is used in the background to convert physics collision data into an audio event. If you want to generate audio outside of a physics-driven simulation context, you don't need to use CollisionEvents.
    /// </summary>
    public readonly struct CollisionEvent
    {
        /// <summary>
        /// The primary object. This should always be the faster-moving object.
        /// </summary>
        public readonly AudioObjectData primary;
        /// <summary>
        /// The secondary object. This should always be the slower-moving (or non-moving) object.
        /// </summary>
        public readonly AudioObjectData secondary;
        /// <summary>
        /// The combined object IDs pair as a ulong. This is used by `AudioGenerator` as a dictionary key.
        /// </summary>
        public readonly ulong ids;
        /// <summary>
        /// The type of the audio event (impact, scrape, roll, none).
        /// </summary>
        public readonly AudioEventType type;
        /// <summary>
        /// The position of the collision.
        /// </summary>
        public readonly Vector3d position;
        /// <summary>
        /// The speed of the collision in meters per second.
        /// </summary>
        public readonly double speed;


        /// <summary>
        /// (constructor)
        /// </summary>
        /// <param name="primary">The primary object.</param>
        /// <param name="secondary">The secondary object.</param>
        /// <param name="type">The type of the audio event (impact, scrape, roll, none).</param>
        /// <param name="speed">The speed of the collision in meters per second.</param>
        /// <param name="position">The position of the collision.</param>
        public CollisionEvent(AudioObjectData primary, AudioObjectData secondary, AudioEventType type, double speed, Vector3d position)
        {
            this.primary = primary;
            this.secondary = secondary;
            this.type = type;
            this.speed = speed;
            this.position = position;
            ids = 0;
            ids = GetIds(primary, secondary);
        }


        /// <summary>
        /// (constructor)
        /// </summary>
        /// <param name="ids">The object IDs pair.</param>
        /// <param name="primary">The primary object.</param>
        /// <param name="secondary">The secondary object in meters per second.</param>
        /// <param name="type">The type of the audio event (impact, scrape, roll, none).</param>
        /// <param name="speed">The speed of the collision in meters per second.</param>
        /// <param name="position">The position of the collision.</param>
        public CollisionEvent(ulong ids, AudioObjectData primary, AudioObjectData secondary, AudioEventType type, double speed, Vector3d position)
        {
            this.ids = ids;
            this.primary = primary;
            this.secondary = secondary;
            this.type = type;
            this.speed = speed;
            this.position = position;
        }
        
        
        /// <summary>
        /// Returns an object ID pair as a ulong. Source: https://stackoverflow.com/a/827267
        /// </summary>
        /// <param name="primary">The primary object.</param>
        /// <param name="secondary">The secondary object.</param>
        public static ulong GetIds(AudioObjectData primary, AudioObjectData secondary)
        {
            return ((ulong)primary.id << 32) | secondary.id;
        }
    }
}