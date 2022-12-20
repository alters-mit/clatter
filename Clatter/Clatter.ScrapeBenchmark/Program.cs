using System.Diagnostics;
using Clatter.Core;


namespace Clatter.ScrapeBenchmark
{
    public class Program
    {
        private static void Main(string[] args)
        {
            Creator.SetPrimaryObject(new AudioObjectData(0, ImpactMaterialSized.metal_1, 0.2f, 0.2f, 1));
            Creator.SetSecondaryObject(new AudioObjectData(1, ImpactMaterialSized.stone_4, 0.5f, 0.1f, 100, ScrapeMaterial.ceramic));
            Creator.SetRandom(0);
            Stopwatch watch = new Stopwatch();
            watch.Start();
            Creator.GetScrape(1f, 10f);
            watch.Stop();
            Console.WriteLine(watch.Elapsed.TotalSeconds);
        }
    }
}