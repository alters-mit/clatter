using System;


namespace Clatter.Core
{
    /// <summary>
    /// Entry point for creating audio.
    /// </summary>
    public static class Creator
    {
        public static byte[] GetImpact(byte primaryImpactMaterial, float primaryAmp, float primaryResonance, float primaryMass, 
            byte secondaryImpactMaterial, float secondaryAmp, float secondaryResonance, float secondaryMass, float speed)
        {
            // Create the objects.
            AudioObjectData a = new AudioObjectData(0, (ImpactMaterialSized)primaryImpactMaterial, primaryAmp, primaryResonance, primaryMass);
            AudioObjectData b = new AudioObjectData(0, (ImpactMaterialSized)secondaryImpactMaterial, secondaryAmp, secondaryResonance, secondaryMass);
            // Create the random number generator.
            Random rng = new Random();
            // Get the impact.
            Impact impact = new Impact(a, b, rng);
            // Get the collision event.
            CollisionEvent collisionEvent = new CollisionEvent(a, b, 0, speed, 0, Vector3d.Zero, OnCollisionType.enter, false);
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


        public static byte[] GetScrape(byte primaryImpactMaterial, float primaryAmp, float primaryResonance, float primaryMass,
            byte secondaryImpactMaterial, float secondaryAmp, float secondaryResonance, float secondaryMass, byte scrapeMaterial, float speed, int count)
        {
            // Get the scrape material.
            ScrapeMaterial sm = (ScrapeMaterial)scrapeMaterial;
            // Create the objects.
            AudioObjectData a = new AudioObjectData(0, (ImpactMaterialSized)primaryImpactMaterial, primaryAmp, primaryResonance, primaryMass);
            AudioObjectData b = new AudioObjectData(0, (ImpactMaterialSized)secondaryImpactMaterial, secondaryAmp, secondaryResonance, secondaryMass, sm);
            a.hasPreviousArea = true;
            a.previousArea = 1;
            a.speed = speed;
            // Create the random number generator.
            Random rng = new Random();
            // Get the scrape.
            Scrape scrape = new Scrape(sm, a, b, rng);
            // Get the collision event.
            CollisionEvent collisionEvent = new CollisionEvent(a, b, 0, speed, 1, Vector3d.Zero, OnCollisionType.stay, false);
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
    }
}