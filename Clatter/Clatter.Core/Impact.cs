using System;
using System.Diagnostics;
using ClatterRs;


namespace Clatter.Core
{
    /// <summary>
    /// Generates impact audio.
    ///
    /// An Impact is actually a *series* of events. By reusing the same Impact object, data from previous impacts can affect the current impact. This is useful for situations such as an object repeatedly bouncing on a table.
    ///
    /// Impact events are automatically generated from collision data within `AudioGenerator`. You can also manually create an Impact and use it to generate audio without needing to use an `AudioGenerator`. This can be useful if you want to generate audio without needing to create a physics simulation.
    ///
    /// ## Code Examples
    ///
    /// {code_example:ImpactAudioExample}
    ///
    /// </summary>
    public class Impact : AudioEvent
    {
        /// <summary>
        /// The maximum contact time in seconds, assuming clampContactTime == true.
        /// </summary>
        private const double MAX_CONTACT_TIME = 2e-3;
        /// <summary>
        /// Clamp amp values to this maximum value.
        /// </summary>
        private const double MAX_AMP = 0.99;
        
        
        /// <summary>
        /// If true, clamp the audio amplitude values to less than or equal to 0.99, preventing distortion.
        /// </summary>
        public static bool preventDistortion = true;
        /// <summary>
        /// If true, clamp the contact time to a plausible value. Set this to false if you want to generate impacts with unusually long contact times.
        /// </summary>
        public static bool clampContactTime = true;
        /// <summary>
        /// The minimum time in seconds between impacts. If an impact occurs an this much time hasn't yet elapsed, the impact will be ignored. This can prevent strange "droning" sounds caused by too many impacts in rapid succession.
        /// </summary>
        public static double minTimeBetweenImpacts = 0.05;
        /// <summary>
        /// The maximum time in seconds between impacts. After this many seconds, this impact series will end and a subsequent impact collision will start a new Impact.
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
        public Impact(ClatterObjectData primary, ClatterObjectData secondary, Random rng) : base(primary, secondary, rng)
        {
            watch.Start();
        }

        
        /// <summary>
        /// Generate audio. Returns true if audio was generated. This will set the `samples` field.
        /// </summary>
        /// <param name="speed">The collision speed in meters per second.</param>
        /// <param name="impulseResponsePath">Optional. If included, this is the path to a file containing impulse response data.</param>
        public override bool GetAudio(double speed, string impulseResponsePath = null)
        {
            // Get the elapsed time.
            dt = watch.Elapsed.TotalSeconds;
            // Prevent droning and/or end the impact event series.
            if (collisionCount > 0 && (dt < minTimeBetweenImpacts || dt > maxTimeBetweenImpacts))
            {
                // End the impact event series.
                if (dt > maxTimeBetweenImpacts)
                {
                    state = EventState.end;
                }
                return false;
            }
            else
            {
                // Adjust the modes and get the amp value.
                double amp = AdjustModes(speed);
                // Get the impulse response.
                int impulseResponseLength = impulseResponsePath == null ? GetImpulseResponse(amp, ref impulseResponse) : LoadImpulseResponse(impulseResponsePath, amp, ref impulseResponse);
                if (impulseResponseLength == 0)
                {
                    return false;
                }
                // Get the contact time.
                double maxT = 0.001 * Math.Min(primary.mass, secondary.mass);
                if (clampContactTime)
                {
                    maxT = Math.Min(maxT, MAX_CONTACT_TIME);
                }
                // Convolve with force, with contact time scaled by the object mass.
                double[] frc;
                if (Globals.CanUseNativeLibrary)
                {
                    frc = new double[(int)Math.Ceiling(maxT * Globals.framerate)];
                    UIntPtr frcLength = (UIntPtr)frc.Length;
                    unsafe
                    {
                        fixed (double* frcPointer = frc)
                        {
                            Vec_double_t frcVec = new Vec_double_t
                            {
                                ptr = frcPointer,
                                len = frcLength,
                                cap = frcLength
                            };
                            Ffi.impact_frequencies(&frcVec, frcLength);
                        }
                    }
                }
                else
                {
                    frc = LinSpace.Get(0, Math.PI, (int)Math.Ceiling(maxT * Globals.framerate));
                    for (int i = 0; i < frc.Length; i++)
                    {
                        frc[i] = Math.Sin(frc[i]);
                    }                 
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
                // Clamp the amp.
                if (preventDistortion && amp > MAX_AMP)
                {
                    amp = MAX_AMP;
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

        
        /// <summary>
        /// Returns the default size of the samples.samples array.
        /// </summary>
        protected override int GetSamplesSize()
        {
            return Globals.DEFAULT_SAMPLES_LENGTH;
        }
    }
}