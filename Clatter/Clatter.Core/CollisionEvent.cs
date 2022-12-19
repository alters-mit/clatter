namespace Clatter.Core
{
    /// <summary>
    /// Data for a collision audio event.
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
        /// The summed object IDs pair as a long.
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
        /// The average normalized speed.
        /// </summary>
        public readonly float normalSpeed;
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
        /// <param name="normalSpeed">The normalized speed of the collision.</param>
        /// <param name="area">The contact area of the collision.</param>
        public CollisionEvent(AudioObjectData collider, AudioObjectData collidee, float angularSpeed, float normalSpeed, double area,
            Vector3d centroid, OnCollisionType onCollisionType, bool filterDuplicates = true)
        {
            ids = 0;
            this.normalSpeed = normalSpeed;
            this.centroid = centroid;
            this.area = area;
            // Compare the IDs for filter out duplicate events.
            if ((filterDuplicates && collider.id > collidee.id) || this.normalSpeed <= 0)
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
        /// From manually set values.
        /// </summary>
        /// <param name="primary">The primary object.</param>
        /// <param name="secondary">The secondary object.</param>
        /// <param name="type">The type of the audio event (impact, scrape, roll, none).</param>
        /// <param name="normalSpeed">The average normalized speed.</param>
        /// <param name="area">The contact area of the collision.</param>
        /// <param name="centroid">The centroid of the collision contact points.</param>
        public CollisionEvent(AudioObjectData primary, AudioObjectData secondary, AudioEventType type, float normalSpeed, float area, Vector3d centroid)
        {
            this.primary = primary;
            this.secondary = secondary;
            this.type = type;
            this.normalSpeed = normalSpeed;
            this.centroid = centroid;
            this.area = area;
            ids = 0;
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