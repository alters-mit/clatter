using System;


namespace Clatter.Core
{
    /// <summary>
    /// Audio data for an impact event.
    /// </summary>
    public class Impact : AudioEvent
    {
        /// <summary>
        /// The minimum time in seconds between impacts.
        /// </summary>
        public static double minTimeBetweenImpacts = 0.25;
        /// <summary>
        /// The maximum time in seconds between impacts.
        /// </summary>
        public static double maxTimeBetweenImpacts = 3;
        /// <summary>
        /// The time of the most recent impact event.
        /// </summary>
        private DateTime time = DateTime.Now;
        /// <summary>
        /// The cached impulse response array.
        /// </summary>
        // ReSharper disable once NotAccessedField.Local
        private double[] impulseResponse;


        /// <summary>
        /// Generate an impact event from object data.
        /// </summary>
        /// <param name="primary">The primary object.</param>
        /// <param name="secondary">The secondary object.</param>
        /// <param name="rng">The random number generator.</param>
        public Impact(AudioObjectData primary, AudioObjectData secondary, Random rng) : base(primary, secondary, rng)
        {
        }


        public override bool GetAudio(CollisionEvent collisionEvent, Random rng)
        {
            // Get the time elapsed between the previous collision and this collision.
            DateTime now = DateTime.Now;
            double t = (now - time).TotalSeconds;
            // Update the impact time.
            time = now;
            // If the latest collision was too recent, ignore this one.
            if (collisionCount > 0 && (t < minTimeBetweenImpacts || t > maxTimeBetweenImpacts))
            {
                return false;
            }
            else
            {
                return GetImpact(collisionEvent, rng, out impulseResponse);
            }
        }
    }
}