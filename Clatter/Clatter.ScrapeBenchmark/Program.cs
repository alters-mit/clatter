using System.Diagnostics;
using Clatter.Core;


namespace Clatter.ScrapeBenchmark
{
    public class Program
    {
        private static void Main(string[] args)
        {
            // Load the materials.
            ScrapeMaterial scrapeMaterial = ScrapeMaterial.ceramic;
            ScrapeMaterialData.Load(scrapeMaterial);
            ImpactMaterialSized primaryImpactMaterial = ImpactMaterialSized.metal_1;
            ImpactMaterialSized secondaryImpactMaterial = ImpactMaterialSized.stone_4;
            ImpactMaterialData.Load(primaryImpactMaterial);
            ImpactMaterialData.Load(secondaryImpactMaterial);
            // Set the objects.
            Creator.SetPrimaryObject(new AudioObjectData(0, primaryImpactMaterial, 0.2f, 0.2f, 1));
            Creator.SetSecondaryObject(new AudioObjectData(1, secondaryImpactMaterial, 0.5f, 0.1f, 100, scrapeMaterial));
            // Generate audio.
            Creator.SetRandom(0);
            Stopwatch watch = new Stopwatch();
            watch.Start();
            Creator.GetScrape(1f, 10f);
            watch.Stop();
            Console.WriteLine(watch.Elapsed.TotalSeconds);
        }
    }
}