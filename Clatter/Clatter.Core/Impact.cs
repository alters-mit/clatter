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
        private double[] impulseResponse = new double[9000];
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

        
        /// <summary>
        /// Generate audio. Returns true if audio was generated. This will set the `samples` field.
        /// </summary>
        /// <param name="speed">The collision speed.</param>
        public override bool GetAudio(double speed)
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
                // Adjust the modes and get the amp value.
                double amp = AdjustModes(speed);
                // Get the impulse response.
                int impulseResponseLength = GetImpulseResponse(amp, ref impulseResponse);
                if (impulseResponseLength == 0)
                {
                    return false;
                }
                // Get the contact time.
                double maxT = 0.001 * Math.Min(primary.mass, secondary.mass);
                if (clampContactTime)
                {
                    maxT = Math.Min(maxT, 2e-3);
                }
                // Convolve with force, with contact time scaled by the object mass.
                double[] frc = LinSpace.Get(0, Math.PI, (int)Math.Ceiling(maxT * Globals.framerate));
                // Clamp the amp.
                if (preventDistortion && amp > 0.99)
                {
                    amp = 0.99;
                }
                for (int i = 0; i < frc.Length; i++)
                {
                    frc[i] = Math.Sin(frc[i]);
                }
                // Convolve.
                impulseResponse.Convolve(frc, impulseResponseLength, ref samples.samples);
                double maxSample = 0;
                for (int i = 0; i < impulseResponseLength; i++)
                {
                    if (samples.samples[i] > maxSample)
                    {
                        maxSample = samples.samples[i];
                    }
                }
                maxSample = Math.Abs(maxSample);
                double maxAbsSample = 0;
                double abs;
                for (int i = 0; i < impulseResponseLength; i++)
                {
                    samples.samples[i] /= maxSample;
                    abs = Math.Abs(samples.samples[i]);
                    if (abs > maxAbsSample)
                    {
                        maxAbsSample = abs;
                    }
                }
                // Scale by the amp value.
                for (int i = 0; i < impulseResponseLength; i++)
                {
                    samples.samples[i] = amp * samples.samples[i] / maxAbsSample;
                }
                samples.length = impulseResponseLength;
                // Restart the clock.
                watch.Restart();
                // Update the collision count.
                collisionCount++;
                return true;
            }
        }

        protected override int GetSamplesSize()
        {
            return Globals.DEFAULT_SAMPLES_LENGTH;
        }
    }
}