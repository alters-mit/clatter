using System.Diagnostics;
using Clatter.Core;


namespace Clatter.ImpactBenchmark
{
    public class Program
    {
        private static void Main(string[] args)
        {
            // Load the materials.
            ImpactMaterial primaryMaterial = ImpactMaterial.glass_1;
            ImpactMaterial secondaryMaterial = ImpactMaterial.stone_4;
            ImpactMaterialData.Load(primaryMaterial);
            ImpactMaterialData.Load(secondaryMaterial);
            // Set the objects.
            AudioObjectData primary = new AudioObjectData(0, primaryMaterial, 0.2, 0.2, 1);
            AudioObjectData secondary = new AudioObjectData(1, secondaryMaterial, 0.5, 0.1, 100);
            // Set some useful globals.
            Random rng = new Random();
            Impact.maxTimeBetweenImpacts = 1000;
            Impact.minTimeBetweenImpacts = 0.0001;
            // Initialize the impact.
            Impact impact = new Impact(primary, secondary, rng);
            // Run the benchmark.
            Stopwatch watch = new Stopwatch();
            watch.Start();
            for (int i = 0; i < 100; i++)
            {
                impact.GetAudio(1, rng);
            }
            watch.Stop();
            Console.WriteLine("Total time: " + watch.Elapsed.TotalSeconds);
        }
    }
}