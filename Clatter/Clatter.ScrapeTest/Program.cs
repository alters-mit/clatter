using Clatter.Core;


namespace Clatter.ScrapeTest
{
    public class Program
    {
        private static void Main(string[] args)
        {
            Creator.SetPrimaryObject(new AudioObjectData(0, ImpactMaterial.metal_1, 0.2f, 0.2f, 1));
            Creator.SetSecondaryObject(new AudioObjectData(1, ImpactMaterial.stone_4, 0.5f, 0.1f, 100, ScrapeMaterial.ceramic));
            Creator.SetRandom(0);
            Creator.WriteScrape(1f, 0.5f, "out.wav");
        }
    }
}