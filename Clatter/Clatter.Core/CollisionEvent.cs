namespace Clatter.Core
{
    /// <summary>
    /// A CollisionEvent derives audio data needed for audio events from a collision. The type and nature of the collision will determine whether the collision generates impact audio, scrape audio, or neither.
    ///
    /// Collisions always have two `AudioObjectData` objects: A collider and a collidee. The faster object is the "primary" object and the slower (or non-moving) object is the "secondary" object. For scrape events, the collidee should be slower and have a `ScrapeMaterial`.
    ///
    /// CollisionEvents use two similar-sounding enum types:
    ///
    /// 1. `AudioEventType` is the type of audio event: impact, scrape, roll, or none. Roll audio is not yet supported.
    /// 2. `OnCollisionType` maps to Unity collision events: enter, stay, exit. An enter is always an impact. A stay can be an impact, scrape, or neither depending on the angular velocity and contact area. An exit is always none.
    ///
    /// The CollisionEvent constructor takes a `OnCollisionType` value and other values such as speed and determines which `AudioEventType` this collision is.
    /// </summary>
    public readonly struct CollisionEvent
    {
        /// <summary>
        /// On a stay event, if the previous area is None and the current area is greater than this, the collision is actually an impact. 
        /// </summary>
        public static double impactAreaNewCollision = 1e-5;
        /// <summary>
        /// On a stay event, if the area of the collision increases by at least this factor, the collision is actually an impact.
        /// </summary>
        public static double impactAreaRatio = 5;
        /// <summary>
        /// On a stay event, if the angular velocity is this or greater, the event is a roll, not a scrape.
        /// </summary>
        public static double rollAngularVelocity = 0.5;
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
        /// The centroid of the collision contact points.
        /// </summary>
        public readonly Vector3d centroid;
        /// <summary>
        /// The speed.
        /// </summary>
        public readonly float speed;
        /// <summary>
        /// The area of the collision.
        /// </summary>
        public readonly double area;

        
        /// <summary>
        /// From collision data.
        /// </summary>
        /// <param name="collider">The collider object.</param>
        /// <param name="collidee">The object that the collider hit.</param>
        /// <param name="angularSpeed">The faster angular speed of the two objects.</param>
        /// <param name="centroid">The centroid of the collision.</param>
        /// <param name="onCollisionType">The type of collision (enter, stay, exit).</param>
        /// <param name="filterDuplicates">If true, try to filter out duplicate collision events.</param>
        /// <param name="speed">The speed of the collision.</param>
        /// <param name="area">The contact area of the collision.</param>
        public CollisionEvent(AudioObjectData collider, AudioObjectData collidee, float angularSpeed, float speed, double area,
            Vector3d centroid, OnCollisionType onCollisionType, bool filterDuplicates = true)
        {
            ids = 0;
            this.speed = speed;
            this.centroid = centroid;
            this.area = area;
            // Compare the IDs for filter out duplicate events.
            if ((filterDuplicates && collider.id > collidee.id) || this.speed <= 0)
            {
                primary = collider;
                secondary = collidee;
                type = AudioEventType.none;
                return;
            }
            // Set the primary and secondary IDs depending on:
            // 1. Whether this is a secondary object.
            // 2. Which object is has a greater speed.
            if (collider.speed > collidee.speed)
            {
                primary = collider;
                secondary = collidee;
            }
            else
            {
                primary = collidee;
                secondary = collider;
            }
            // Exits are always none.
            if (onCollisionType == OnCollisionType.exit)
            {
                type = AudioEventType.none;
            }
            // Enter or stay.
            else
            {
                type = default;
                // Enter events are always impacts.
                if (onCollisionType == OnCollisionType.enter)
                {
                    type = AudioEventType.impact;
                }
                // There is no previous area.
                else if (!primary.hasPreviousArea)
                {
                    // The area is big enough to be an impact.
                    if (area > impactAreaNewCollision)
                    {
                        type = AudioEventType.impact;
                    }
                    // This is a non-event.
                    else
                    {
                        type = AudioEventType.none;
                    }
                }
                // There is previous area, and the ratio between the new area and the previous area is small.
                else if (primary.previousArea > 0 && area / primary.previousArea < impactAreaRatio)
                {
                    // If there is a high angular velocity, this is a roll.
                    if (angularSpeed > rollAngularVelocity)
                    {
                        type = AudioEventType.roll;
                    }
                    // If there is little angular velocity, this is a scrape.
                    else
                    {
                        type = AudioEventType.scrape;
                    }
                }
                // This is a non-event.
                else
                {
                    type = AudioEventType.none;
                }
            }
            ids = GetIDsKey();
        }


        /// <summary>
        /// Get an object ID pair as a long. Source: https://stackoverflow.com/a/827267
        /// </summary>
        private ulong GetIDsKey()
        {
            // Shift the bits creating an empty space on the right.
            // ex: 0x0000CFFF becomes 0xCFFF0000
            // Combine the bits on the right with the previous value.
            // ex: 0xCFFF0000 | 0x0000ABCD becomes 0xCFFFABCD
            return ((ulong)primary.id << 32) | secondary.id;
        }
    }
}