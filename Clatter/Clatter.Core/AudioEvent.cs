using System;


namespace Clatter.Core
{
    /// <summary>
    /// Data for a physics-derived audio event.
    /// </summary>
    public abstract class AudioEvent
    {
        /// <summary>
        /// The overall amp of the simulation.
        /// </summary>
        public static double initialAmp = 0.5;
        /// <summary>
        /// If true, clamp amp values to 0.99.
        /// </summary>
        public static bool preventDistortion = true;
        /// <summary>
        /// If true, clamp the impulse contact time.
        /// </summary>
        public static bool clampContactTime = true;
        /// <summary>
        /// The audio samples.
        /// </summary>
        public readonly Samples samples = new Samples();
        /// <summary>
        /// The current state of the audio event.
        /// </summary>
        public EventState state = EventState.start;
        /// <summary>
        /// The collision counter.
        /// </summary>
        protected int collisionCount;
        /// <summary>
        /// The amplitude of the first collision.
        /// </summary>
        private double amp;
        /// <summary>
        /// The speed of the initial collision.
        /// </summary>
        private double initialSpeed = 1;
        /// <summary>
        /// The modes of the first object.
        /// </summary>
        private readonly Modes modesA;
        /// <summary>
        /// The modes of the second object.
        /// </summary>
        private readonly Modes modesB;


        /// <summary>
        /// Generate an audio event from object data.
        /// </summary>
        /// <param name="primary">The primary object.</param>
        /// <param name="secondary">The secondary object.</param>
        /// <param name="rng">The random number generator.</param>
        public AudioEvent(AudioObjectData primary, AudioObjectData secondary, Random rng)
        {
            // Generate the modes.
            modesB = new Modes(ImpactMaterialData.impactMaterials[primary.impactMaterial], rng);
            modesA = new Modes(ImpactMaterialData.impactMaterials[secondary.impactMaterial], rng);
            amp = primary.amp * initialAmp;
        }


        /// <summary>
        /// Generate audio. Returns true if audio was generated. This will set the `samples` field.
        /// </summary>
        /// <param name="collisionEvent">The collision event. Contains data for this specific collision.</param>
        /// <param name="rng">The random number generator.</param>
        public abstract bool GetAudio(CollisionEvent collisionEvent, Random rng);


        /// <summary>
        /// Synthesize impact audio. Returns true if successful.
        /// </summary>
        /// <param name="collisionEvent">The collision event. Contains data for this specific collision.</param>
        /// <param name="rng">The random number generator.</param>
        /// <param name="impulseResponse">The impulse response.</param>
        protected bool GetImpact(CollisionEvent collisionEvent, Random rng, out double[] impulseResponse)
        {
            // ReSharper disable once LocalVariableHidesMember
            double amp;
            // Re-scale the amplitude.
            if (collisionCount == 0)
            {
                double log10RelativeAmp = 20 * Math.Log10(collisionEvent.secondary.amp / collisionEvent.primary.amp);
                for (int i = 0; i < modesB.decayTimes.Length; i++)
                {
                    modesB.decayTimes[i] += log10RelativeAmp;
                }
                // Set the amp.
                this.amp = collisionEvent.primary.amp * initialAmp;
                amp = this.amp;
                // Set the initial speed.
                initialSpeed = collisionEvent.normalSpeed;
            }
            else
            {
                // Set the amp.
                amp = this.amp * collisionEvent.normalSpeed / initialSpeed;
                // Adjust modes so that two successive impacts are not identical.
                modesA.AdjustPowers(rng);
                modesB.AdjustPowers(rng);
            }
            // Generate the sound.
            double[] rawSamples;
            SynthImpactModes(collisionEvent, amp, out rawSamples, out impulseResponse);
            // This should rarely happen, if ever.
            if (rawSamples == null)
            {
                return false;
            }
            // Update the samples.
            samples.Set(rawSamples, 0, rawSamples.Length);
            // Update the collision count.
            collisionCount++;
            return true;
        }


        /// <summary>
        /// Synth impact modes.
        /// </summary>
        /// <param name="collisionEvent">The collision audio event.</param>
        /// <param name="amp">The audio amp.</param>
        /// <param name="samples">The samples.</param>
        /// <param name="impulseResponse">The impulse response.</param>
        private void SynthImpactModes(CollisionEvent collisionEvent, double amp, out double[] samples, out double[] impulseResponse)
        {
            impulseResponse = Modes.Add(modesA.Sum(collisionEvent.primary.resonance),
                modesB.Sum(collisionEvent.secondary.resonance));
            if (impulseResponse.Length == 0)
            {
                samples = null;
                return;
            }
            // Get the contact time.
            double maxT = 0.001 * Math.Min(collisionEvent.primary.mass, collisionEvent.secondary.mass);
            if (clampContactTime)
            {
                maxT = Math.Min(maxT, 2e-3);
            }
            // Convolve with force, with contact time scaled by the object mass.
            double[] frc = Util.LinSpace(0, Math.PI, (int)Math.Ceiling(maxT * Globals.framerate));
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
            samples = impulseResponse.Convolve(frc, false);
            double maxSample = 0;
            for (int i = 0; i < samples.Length; i++)
            {
                if (samples[i] > maxSample)
                {
                    maxSample = samples[i];
                }
            }
            maxSample = Math.Abs(maxSample);
            double maxAbsSample = 0;
            double abs;
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] /= maxSample;
                abs = Math.Abs(samples[i]);
                if (abs > maxAbsSample)
                {
                    maxAbsSample = abs;
                }
            }
            // Scale by the amp value.
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = amp * samples[i] / maxAbsSample;
            }
        }
    }
}