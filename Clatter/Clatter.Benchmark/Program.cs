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
            path = Path.GetFullPath(Path.Combine(path.Split("/clatter/")[0], "clatter/docs/benchmark.md"));
            string text = File.ReadAllText(path).Split("**RESULTS:**")[0].Trim() + "\n\n**RESULTS:**\n\n";
            double impact = ImpactBenchmark();
            double scrape = ScrapeBenchmark();
            double threadedTotal;
            double threadedAverage;
            ThreadedBenchmark(out threadedTotal, out threadedAverage);
            string table = "| Benchmark | Time (seconds) |\n| --- | --- |\n| Impact | " + impact + " |\n| Scrape | " +
                    scrape + " |\n| Threaded (total) | " + threadedTotal + " |\n| Threaded (average) | " +
                    threadedAverage + " |"; 
            Console.WriteLine(table);
            File.WriteAllText(path, text + table);
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
            Random rng = new Random(0);
            Impact.maxTimeBetweenImpacts = 1000;
            Impact.minTimeBetweenImpacts = 0;
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
            Random rng = new Random(0);
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


        private static void ThreadedBenchmark(out double totalElapsed, out double averageElapsed)
        {
            // Load the materials.
            ImpactMaterial primaryMaterial = ImpactMaterial.glass_1;
            ImpactMaterial secondaryMaterial = ImpactMaterial.stone_4;
            ImpactMaterialData.Load(primaryMaterial);
            ImpactMaterialData.Load(secondaryMaterial);
            // Set some useful globals.
            Impact.maxTimeBetweenImpacts = 1000;
            Impact.minTimeBetweenImpacts = 0.0001;
            Vector3d zero = Vector3d.Zero;
            Stopwatch watch = new Stopwatch();
            double[] times = new double[100];
            for (int i = 0; i < 100; i++)
            {
                // Set the objects.
                AudioObjectData[,] objects = new AudioObjectData[100, 2];
                List<AudioObjectData> objectsFlat = new List<AudioObjectData>();
                uint id = 0;
                for (int j = 0; j < 100; j++)
                {
                    AudioObjectData primary = new AudioObjectData(id, primaryMaterial, 0.2, 0.2, 1);
                    id++;
                    AudioObjectData secondary = new AudioObjectData(id, secondaryMaterial, 0.5, 0.1, 100);
                    id++;
                    objects[j, 0] = primary;
                    objects[j, 1] = secondary;
                }
                // Set the generator.
                AudioGenerator generator = new AudioGenerator(objectsFlat.ToArray());
                Random rng = new Random(i);
                // Add the collisions.
                for (int j = 0; j < 100; j++)
                {
                    generator.AddCollision(new CollisionEvent(objects[j, 0], objects[j, 1], AudioEventType.impact, rng.NextDouble() * 1.75, zero));
                }
                watch.Start();
                generator.Update();
                watch.Stop();
                times[i] = watch.Elapsed.TotalSeconds;
                watch.Reset();
            }
            totalElapsed = times.Sum();
            averageElapsed = totalElapsed / times.Length;
        }
    }
}

