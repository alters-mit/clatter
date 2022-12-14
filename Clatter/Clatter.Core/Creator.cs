using System;


namespace Clatter.Core
{
    /// <summary>
    /// Entry point for creating audio.
    /// </summary>
    public static class Creator
    {
        /// <summary>
        /// The primary object.
        /// </summary>
        private static AudioObjectData primary;
        /// <summary>
        /// The secondary object.
        /// </summary>
        private static AudioObjectData secondary;
        /// <summary>
        /// A cached impact. This can be used for ongoing impacts.
        /// </summary>
        private static Impact impact;
        
        
        /// <summary>
        /// Set the primary object.
        /// </summary>
        /// <param name="impactMaterial">A byte representing the impact material.</param>
        /// <param name="amp">The amp.</param>
        /// <param name="resonance">The resonance.</param>
        /// <param name="mass">The mass.</param>
        public static void SetPrimaryObject(byte impactMaterial, float amp, float resonance, float mass)
        {
            primary = new AudioObjectData(0, (ImpactMaterialSized)impactMaterial, amp, resonance, mass);
        }


        /// <summary>
        /// Set the secondary object.
        /// </summary>
        /// <param name="impactMaterial">A byte representing the impact material.</param>
        /// <param name="amp">The amp.</param>
        /// <param name="resonance">The resonance.</param>
        /// <param name="mass">The mass.</param>
        /// <param name="scrapeMaterial">The scrape material. Can be null.</param>
        public static void SetSecondaryObject(byte impactMaterial, float amp, float resonance, float mass, byte? scrapeMaterial)
        {
            secondary = new AudioObjectData(1, (ImpactMaterialSized)impactMaterial, amp, resonance, mass,
                (ScrapeMaterial?)scrapeMaterial);
        }


        /// <summary>
        /// Generate an impact sound.
        /// This assumes that you have called SetPrimaryObject and SetSecondaryObject.
        /// </summary>
        /// <param name="speed">The speed of the impact.</param>
        /// <param name="newImpact">If true, this is a new impact. If false, this is part of an ongoing series of impacts. Ignored the first time this function is called.</param>
        public static byte[] GetImpact(float speed, bool newImpact)
        {
            // Create the random number generator.
            Random rng = new Random();
            // Set the impact.
            if (newImpact || impact == null)
            {
                impact = new Impact(primary, secondary, rng);
            }
            // Get the collision event.
            CollisionEvent collisionEvent = new CollisionEvent(primary, secondary, 0, speed, 0, Vector3d.Zero, OnCollisionType.enter, false);
            // Generate audio.
            bool ok = impact.GetAudio(collisionEvent, rng);
            if (!ok)
            {
                return new byte[0];
            }
            else
            {
                return impact.samples.ToInt16Bytes();
            }
        }


        /// <summary>
        /// Generate a scrape sound.
        /// This assumes that you have called SetPrimaryObject and SetSecondaryObject, and that in the latter call you provided a scrape material.
        /// </summary>
        /// <param name="speed">The speed of the scrape.</param>
        /// <param name="duration">The duration of the scrape in seconds. This will be rounded to the nearest tenth of a second.</param>
        public static byte[] GetScrape(float speed, float duration)
        {
            int count = (int)(duration * Globals.framerate / Scrape.SAMPLES_LENGTH);
            // Get the scrape material.
            // Create the objects.
            primary.hasPreviousArea = true;
            primary.previousArea = 1;
            primary.speed = speed;
            // Create the random number generator.
            Random rng = new Random();
            // Get the scrape.
            Scrape scrape = new Scrape(secondary.scrapeMaterial, primary, secondary, rng);
            // Get the collision event.
            CollisionEvent collisionEvent = new CollisionEvent(primary, secondary, 0, speed, 1, Vector3d.Zero, OnCollisionType.stay, false);
            byte[] audio = new byte[Scrape.SAMPLES_LENGTH * 2 * count];
            for (int i = 0; i < count; i++)
            {
                // Continue the scrape.
                scrape.GetAudio(collisionEvent, rng);
                // Get the audio and copy it to the buffer.
                Buffer.BlockCopy(scrape.samples.ToInt16Bytes(), 0, audio, i * 2 * count, Scrape.SAMPLES_LENGTH * 2);
            }
            return audio;
        }
        
        
        /// <summary>
        /// Generate an impact sound and write it to disk as a .wav file.
        /// </summary>
        /// <param name="speed">The speed of the impact.</param>
        /// <param name="newImpact">If true, this is a new impact. If false, this is part of an ongoing series of impacts. Ignored the first time this function is called.</param>
        /// <param name="path">The path to the output file.</param>
        public static void WriteImpact(float speed, bool newImpact, string path)
        {
            WavWriter writer = new WavWriter(path);
            writer.Write(GetImpact(speed, newImpact));
            writer.End();
        }
        
        
        /// <summary>
        /// Generate a scrape sound and write it to disk as a .wav file.
        /// This assumes that you have called SetPrimaryObject and SetSecondaryObject, and that in the latter call you provided a scrape material.
        /// </summary>
        /// <param name="speed">The speed of the scrape.</param>
        /// <param name="duration">The duration of the scrape in seconds. This will be rounded to the nearest tenth of a second.</param>
        /// <param name="path">The path to the output file.</param>
        public static void WriteScrape(float speed, float duration, string path)
        {
            WavWriter writer = new WavWriter(path);
            writer.Write(GetScrape(speed, duration));
            writer.End();
        }
    }
}