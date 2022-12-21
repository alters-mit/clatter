using System;
using System.Diagnostics;


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
        /// The cached impulse response array.
        /// </summary>
        // ReSharper disable once NotAccessedField.Local
        private double[] impulseResponse;
        /// <summary>
        /// The stopwatch used to record time.
        /// </summary>
        private readonly Stopwatch watch = new Stopwatch();
        /// <summary>
        /// A cached double for elapsed time.
        /// </summary>
        private double dt;


        /// <summary>
        /// (constructor)
        /// </summary>
        /// <param name="primary">The primary object.</param>
        /// <param name="secondary">The secondary object.</param>
        /// <param name="rng">The random number generator.</param>
        public Impact(AudioObjectData primary, AudioObjectData secondary, Random rng) : base(primary, secondary, rng)
        {
            watch.Start();
        }


        public override bool GetAudio(CollisionEvent collisionEvent, Random rng)
        {
            // Get the elapsed time.
            dt = watch.Elapsed.TotalSeconds;
            // If the latest collision was too recent, ignore this one.
            if (collisionCount > 0 && (dt < minTimeBetweenImpacts || dt > maxTimeBetweenImpacts))
            {
                return false;
            }
            else
            {
                // Restart the clock.
                watch.Restart();
                // Get an impact.
                return GetImpact(collisionEvent, rng, out impulseResponse);
            }
        }
    }
}