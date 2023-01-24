using System;
using System.IO;
using Clatter.Core;

public static class AudioGeneratorImpact
{
    private static AudioGenerator generator;
    private static Queue<byte[]> audioData = new Queue<byte[]>();
    
    public static void Main(string[] args)
    {
        Random rng = new Random();
        // Add some objects.
        ClatterObjectData[] objects = new ClatterObjectData[64];
        uint[] objectIDs = new uint[64];
        for (uint i = 0; i < objects.Length; i++)
        {
            // Generate a random object.
            objects[i] = new ClatterObjectData(i, ImpactMaterial.glass_1, rng.NextDouble(), rng.NextDouble(), rng.NextDouble() * 5);
            objectIDs[i] = i;
        }
        // Create the audio generator.
        generator = new AudioGenerator(objects);
        // Listen for impact events.
        generator.onImpact += OnImpact;
        // Get the output directory.
        string outputDirectory = Path.GetFullPath("output");
        // Iterate for 15 frames.
        for (int i = 0; i < 15; i++)
        {
            // Get a random number of collisions.
            int numCollisions = rng.Next(15, 30);
            // Randomize the object IDs.
            objectIDs = objectIDs.OrderBy(x => rng.NextDouble()).ToArray();
            int objectIndex = 0;
            // Generate collisions.
            for (int j = 0; j < numCollisions; j++)
            {
                // Get random primary and secondary objects.
                ClatterObjectData primary = objects[objectIDs[objectIndex]];
                ClatterObjectData secondary = objects[objectIDs[objectIndex + 1]];
                // Generate a collision.
                CollisionEvent collisionEvent = new CollisionEvent(primary, secondary, AudioEventType.impact, rng.NextDouble() * 1.75, Vector3d.Zero);
                // Add the collision.
                generator.AddCollision(collisionEvent);
                // Increment the object index for the next collision.
                objectIndex += 2;
                if (objectIndex >= objects.Length)
                {
                    objectIndex = 0;
                }
            }
            // Update.
            generator.Update();
            // Write the audio wav data.
            int audioIndex = 0;
            while (audioData.Count > 0)
            {
                WavWriter writer = new WavWriter(Path.Combine(outputDirectory, audioIndex + ".wav"));
                writer.Write(audioData.Dequeue());
                writer.End();
                audioIndex++;
            }
        }
    }


    private static void OnImpact(CollisionEvent collisionEvent, Samples samples, Vector3d centroid, int audioSourceId)
    {
        audioData.Enqueue(samples.ToInt16Bytes());
    }
}