using System.Diagnostics;
using Clatter.Core;


namespace Clatter.ImpactBenchmark
{
    public class Program
    {
        private static void Main(string[] args)
        {
            Creator.SetPrimaryObject(new AudioObjectData(0, ImpactMaterialSized.glass_1, 0.2f, 0.2f, 1));
            Creator.SetSecondaryObject(new AudioObjectData(1, ImpactMaterialSized.stone_4, 0.5f, 0.1f, 100));
            Impact.maxTimeBetweenImpacts = 1000;
            Impact.minTimeBetweenImpacts = 0.0001;
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