using System;


namespace Clatter.Core
{
    /// <summary>
    /// AudioEvent is an abstract base class for generating audio from physics events. See: `Impact` and `Scrape`.
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
        /// The minimum collision speed.
        /// </summary>
        public static double minSpeed = 0.00001;
        /// <summary>
        /// The audio samples generated from this event.
        /// </summary>
        public readonly Samples samples;
        /// <summary>
        /// The current state of the AudioEvent. This is not the same thing as whether any audio is playing. An `Impact` ends when too much time has elapsed since the most recent impact collision. A `Scrape` ends when the object is moving too slowly.
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
        /// The random number generator. Each audio event has its own Random object for thread safety.
        /// </summary>
        protected readonly Random rng;
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
        /// <param name="rng">The random number generator.</param>
        protected AudioEvent(AudioObjectData primary, AudioObjectData secondary, Random rng)
        {
            samples = new Samples(GetSamplesSize());
            this.primary = primary;
            this.secondary = secondary;
            this.rng = rng;
            // Generate the modes.
            modesB = new Modes(ImpactMaterialData.impactMaterials[primary.impactMaterial], rng);
            modesA = new Modes(ImpactMaterialData.impactMaterials[secondary.impactMaterial], rng);
            initialAmp = primary.amp * simulationAmp;
        }


        /// <summary>
        /// Generate audio. Returns true if audio was generated. This will set the `samples` field.
        /// </summary>
        /// <param name="speed">The collision speed.</param>
        public abstract bool GetAudio(double speed);
        
        
        /// <summary>
        /// Randomly adjust Modes values. Returns the new amp value.
        /// </summary>
        /// <param name="speed">The speed of the collision.</param>
        protected double AdjustModes(double speed)
        {
            // Re-scale the amplitude.
            double amp;
            if (collisionCount == 0)
            {
                // Set initial modes data.
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
            return amp;
        }
        
        
        /// <summary>
        /// Returns an impulse response array.
        /// </summary>
        /// <param name="amp">The amplitude multiplier of the sound.</param>
        /// <param name="impulseResponse">The generated impulse response.</param>
        /// <returns></returns>
        protected int GetImpulseResponse(double amp, ref double[] impulseResponse)
        {
            if (amp <= 0)
            {
                return 0;
            }
            // Sum the modes.
            modesA.Sum(primary.resonance);
            modesB.Sum(secondary.resonance);
            int impulseResponseLength = Modes.Add(modesA.synthSound, modesA.synthSoundLength, modesB.synthSound, modesB.synthSoundLength, ref impulseResponse);
            return impulseResponseLength;
        }


        /// <summary>
        /// Returns the default size of the samples.samples array.
        /// </summary>
        protected abstract int GetSamplesSize();
    }
}