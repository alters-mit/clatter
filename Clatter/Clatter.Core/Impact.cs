using System;
using System.Diagnostics;


namespace Clatter.Core
{
    /// <summary>
    /// Impact is a subclass of `AudioEvent` used to generate impact audio.
    ///
    /// An Impact is actually a *series* of events. By reusing the same Impact object, data from previous impacts can affect the current impact. This is useful for situations such as an object repeatedly bouncing on a table.
    ///
    /// This is a minimal example of how to generate impact audio:
    ///
    /// ```csharp
    /// using System;
    /// using Clatter.Core;
    ///
    /// rng = new Random();
    /// AudioObjectData primary = new AudioObjectData(0, ImpactMaterialSized.glass_1, 0.2f, 0.2f, 1);
    /// AudioObjectData secondary = new AudioObjectData(1, ImpactMaterialSized.stone_4, 0.5f, 0.1f, 100);
    /// Impact impact = new Impact(primary, secondary, rng);
    /// impact.GetAudio(1, rng);
    /// byte[] wavData = impact.samples.ToInt16Bytes();
    /// ```
    /// </summary>
    public class Impact : AudioEvent
    {
        /// <summary>
        /// The minimum time in seconds between impacts. This can prevent strange "droning" sounds caused by too many impacts in rapid succession.
        /// </summary>
        public static double minTimeBetweenImpacts = 0.25;
        /// <summary>
        /// The maximum time in seconds between impacts. This can prevent strange "droning" sounds caused by too many impacts in rapid succession.
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
        /// <param name="rng">The random number generator. This is used to randomly adjust audio data before generating new audio.</param>
        public Impact(AudioObjectData primary, AudioObjectData secondary, Random rng) : base(primary, secondary, rng)
        {
            watch.Start();
        }


        public override bool GetAudio(float speed, Random rng)
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
                return GetImpact(speed, rng, out impulseResponse);
            }
        }
    }
}