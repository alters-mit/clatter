using System.Diagnostics;
using Clatter.Core;


namespace Clatter.ImpactBenchmark
{
    public class Program
    {
        private static void Main(string[] args)
        {
            // Load the materials.
            ImpactMaterialSized primaryMaterial = ImpactMaterialSized.glass_1;
            ImpactMaterialSized secondaryMaterial = ImpactMaterialSized.stone_4;
            ImpactMaterialData.Load(primaryMaterial);
            ImpactMaterialData.Load(secondaryMaterial);
            // Set the objects.
            Creator.SetPrimaryObject(new AudioObjectData(0, primaryMaterial, 0.2f, 0.2f, 1));
            Creator.SetSecondaryObject(new AudioObjectData(1, secondaryMaterial, 0.5f, 0.1f, 100));
            // Set some useful globals.
            Creator.SetRandom(0);
            Impact.maxTimeBetweenImpacts = 1000;
            Impact.minTimeBetweenImpacts = 0.0001;
            // Run the benchmark.
            Stopwatch watch = new Stopwatch();
            watch.Start();
            for (int i = 0; i < 100; i++)
            {
                Creator.GetImpact(1, i == 0);       
            }
            watch.Stop();
            Console.WriteLine("Total time: " + watch.Elapsed.TotalSeconds);
        }
    }
}