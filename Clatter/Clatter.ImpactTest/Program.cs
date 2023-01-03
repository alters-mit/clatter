using Clatter.Core;


namespace Clatter.ImpactTest
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
            Impact impact = new Impact(primary, secondary, rng);
            impact.GetAudio(1, rng);
            WavWriter writer = new WavWriter("out.wav", overwrite: true);
            writer.Write(impact.samples.ToInt16Bytes());
            writer.End();
        }
    }
}