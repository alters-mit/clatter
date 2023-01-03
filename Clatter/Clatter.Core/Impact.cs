using System;
using System.Diagnostics;


namespace Clatter.Core
{
    /// <summary>
    /// Impact is a subclass of `AudioEvent` used to generate impact sounds.
    ///
    /// An Impact is actually a *series* of events. By reusing the same Impact object, data from previous impacts can affect the current impact. This is useful for situations such as an object repeatedly bouncing on a table.
    ///
    /// Impact events are automatically generate from collision data within `AudioGenerator`. You can also manually create an Impact and use it to generate audio without needing to use an `AudioGenerator`. This can be useful if you want to generate audio without needing to create a physics simulation:
    ///
    /// {code_example:ImpactAudioExample}
    /// 
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


        public override bool GetAudio(double speed, Random rng)
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