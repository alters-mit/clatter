using System.Diagnostics;
using Clatter.Core;


namespace Clatter.Benchmark
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Get the path to the output.
            string path = Path.GetFullPath(".").Replace("\\", "/");
            path = Path.GetFullPath(Path.Combine(path.Split("/clatter/")[0], "clatter/docs/benchmark.txt"));
            double impact = ImpactBenchmark();
            double scrape = ScrapeBenchmark();
            string text = "impact=" + impact + "\nscrape=" + scrape;
            File.WriteAllText(path, text);
            Console.WriteLine(text);
        }
        
        
        private static double ImpactBenchmark()
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
            return watch.Elapsed.TotalSeconds;
        }


        private static double ScrapeBenchmark()
        {
            // Load the materials.
            ImpactMaterial primaryMaterial = ImpactMaterial.glass_1;
            ImpactMaterial secondaryMaterial = ImpactMaterial.stone_4;
            ScrapeMaterial scrapeMaterial = ScrapeMaterial.ceramic;
            ImpactMaterialData.Load(primaryMaterial);
            ImpactMaterialData.Load(secondaryMaterial);
            ScrapeMaterialData.Load(scrapeMaterial);
            // Set the objects.
            AudioObjectData primary = new AudioObjectData(0, primaryMaterial, 0.2, 0.2, 1);
            AudioObjectData secondary = new AudioObjectData(1, secondaryMaterial, 0.5, 0.1, 100, scrapeMaterial);
            // Initialize the scrape.
            Random rng = new Random();
            Scrape scrape = new Scrape(scrapeMaterial, primary, secondary, rng);
            // Generate the scrape.
            int count = Scrape.GetNumScrapeEvents(10);
            Stopwatch watch = new Stopwatch();
            watch.Start();
            for (int i = 0; i < count; i++)
            {
                scrape.GetAudio(1, rng);
            }
            watch.Stop();
            return watch.Elapsed.TotalSeconds;
        }
    }
}

