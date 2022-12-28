using System;


namespace Clatter.Core
{
    /// <summary>
    /// AudioEvent is an abstract base class for generating audio from physics events.
    /// </summary>
    public abstract class AudioEvent
    {
        /// <summary>
        /// The overall amplitude of the simulation. The amplitude of generated audio is scaled by this factor. Must be between 0 and 0.99
        /// </summary>
        public static double simulationAmp = 0.5;
        /// <summary>
        /// If true, clamp the audio amplitude values to 0.99.
        /// </summary>
        public static bool preventDistortion = true;
        /// <summary>
        /// If true, clamp the impulse contact time.
        /// </summary>
        public static bool clampContactTime = true;
        /// <summary>
        /// The audio samples generated from this event.
        /// </summary>
        public readonly Samples samples = new Samples();
        /// <summary>
        /// The current state of the AudioEvent.
        /// </summary>
        public EventState state = EventState.start;
        /// <summary>
        /// The number of collision events in this series so far.
        /// </summary>
        protected int collisionCount;
        /// <summary>
        /// The primary object.
        /// </summary>
        protected readonly AudioObjectData primary;
        /// <summary>
        /// The secondary object.
        /// </summary>
        protected readonly AudioObjectData secondary;
        /// <summary>
        /// The amplitude of the first collision.
        /// </summary>
        private double initialAmp;
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
        /// <param name="rng">The random number generator. This is used to randomly adjust audio data before generating new audio.</param>
        protected AudioEvent(AudioObjectData primary, AudioObjectData secondary, Random rng)
        {
            this.primary = primary;
            this.secondary = secondary;
            // Generate the modes.
            modesB = new Modes(ImpactMaterialData.impactMaterials[primary.impactMaterial], rng);
            modesA = new Modes(ImpactMaterialData.impactMaterials[secondary.impactMaterial], rng);
            initialAmp = primary.amp * simulationAmp;
        }


        /// <summary>
        /// Generate audio. Returns true if audio was generated. This will set the `samples` field.
        /// </summary>
        /// <param name="speed">The collision speed.</param>
        /// <param name="rng">The random number generator.</param>
        public abstract bool GetAudio(float speed, Random rng);


        /// <summary>
        /// Synthesize impact audio. Returns true if successful.
        /// </summary>
        /// <param name="speed">The collision speed.</param>
        /// <param name="rng">The random number generator.</param>
        /// <param name="impulseResponse">The impulse response.</param>
        protected bool GetImpact(float speed, Random rng, out double[] impulseResponse)
        {
            // ReSharper disable once LocalVariableHidesMember
            double amp;
            // Re-scale the amplitude.
            if (collisionCount == 0)
            {
                double log10RelativeAmp = 20 * Math.Log10(secondary.amp / primary.amp);
                for (int i = 0; i < modesB.decayTimes.Length; i++)
                {
                    modesB.decayTimes[i] += log10RelativeAmp;
                }
                // Set the amp.
                initialAmp = primary.amp * simulationAmp;
                amp = initialAmp;
                // Set the initial speed.
                initialSpeed = speed;
            }
            else
            {
                // Set the amp.
                amp = initialAmp * speed / initialSpeed;
                // Adjust modes so that two successive impacts are not identical.
                modesA.AdjustPowers(rng);
                modesB.AdjustPowers(rng);
            }
            // Generate the sound.
            double[] rawSamples;
            // This should rarely happen, if ever.
            if (!SynthImpactModes(amp, out rawSamples, out impulseResponse))
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
        /// <param name="amp">The audio amp.</param>
        /// <param name="samples">The samples.</param>
        /// <param name="impulseResponse">The impulse response.</param>
        private bool SynthImpactModes(double amp, out double[] samples, out double[] impulseResponse)
        {
            if (amp <= 0)
            {
                samples = null;
                impulseResponse = null;
                return false;
            }
            impulseResponse = Array.Empty<double>();
            // Sum the modes.
            modesA.Sum(primary.resonance);
            modesB.Sum(secondary.resonance);
            int impulseResponseLength = Modes.Add(modesA.synthSound, modesA.synthSoundLength, modesB.synthSound, modesB.synthSoundLength, ref impulseResponse);
            if (impulseResponseLength == 0)
            {
                samples = null;
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
            samples = impulseResponse.Convolve(frc, impulseResponseLength);
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
            return true;
        }
    }
}