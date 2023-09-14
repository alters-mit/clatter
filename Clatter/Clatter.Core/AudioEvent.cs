using System;
using System.IO;


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
        public static double simulationAmp = 0.9;
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
        protected readonly ClatterObjectData primary;
        /// <summary>
        /// The secondary object.
        /// </summary>
        protected readonly ClatterObjectData secondary;
        /// <summary>
        /// The random number generator. Each audio event has its own Random object for thread safety.
        /// </summary>
        private readonly Random rng;
        /// <summary>
        /// The amplitude of the first collision.
        /// </summary>
        private double initialAmp;
        /// <summary>
        /// The speed of the initial collision in meters per second.
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
        protected AudioEvent(ClatterObjectData primary, ClatterObjectData secondary, Random rng)
        {
            // ReSharper disable once VirtualMemberCallInConstructor
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
        /// <param name="speed">The collision speed in meters per second.</param>
        /// <param name="impulseResponsePath">Optional. If included, this is the path to a file containing impulse response data.</param>
        public abstract bool GetAudio(double speed, string impulseResponsePath = null);
        
        
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
                modesB.AddAmpToDecayTimes(20 * Math.Log10(secondary.amp / primary.amp));
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
        /// Generate an impulse response. Returns the length of the data within the impulseResponse array.
        /// </summary>
        /// <param name="amp">The amplitude multiplier of the sound.</param>
        /// <param name="impulseResponse">The generated impulse response. The returned integer is the length of the data.</param>
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
        /// Load an impulse response from a file.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <param name="amp">The amplitude multiplier of the sound.</param>
        /// <param name="impulseResponse">The generated impulse response. The returned integer is the length of the data.</param>
        protected int LoadImpulseResponse(string path, double amp, ref double[] impulseResponse)
        {
            if (amp <= 0)
            {
                return 0;
            }
            // Sum the modes.
            modesA.Sum(primary.resonance);
            modesB.Sum(secondary.resonance);
            
            // Load the impulse response.
            byte[] impulseResponseBytes = File.ReadAllBytes(path);
            int impulseResponseLength = impulseResponseBytes.Length / 8;
            
            // Resize the output array if needed.
            if (impulseResponse.Length < impulseResponseLength)
            {
                Array.Resize(ref impulseResponse, impulseResponseLength);;
            }
            
            // Copy the bytes.
            Buffer.BlockCopy(impulseResponseBytes, 0, impulseResponse, 0, impulseResponseLength);
            return impulseResponseLength;
        }


        /// <summary>
        /// Returns the default size of the samples.samples array.
        /// </summary>
        protected abstract int GetSamplesSize();
    }
}