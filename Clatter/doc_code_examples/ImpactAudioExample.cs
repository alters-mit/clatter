using Clatter.Core;

public class ImpactAudioExample
{
    private static void Main(string[] args)
    {
        // Load the materials.
        ImpactMaterial primaryMaterial = ImpactMaterial.glass_1;
        ImpactMaterial secondaryMaterial = ImpactMaterial.stone_4;
        ImpactMaterialData.Load(primaryMaterial);
        ImpactMaterialData.Load(secondaryMaterial);
        // Set the objects.
        ClatterObjectData primary = new ClatterObjectData(0, primaryMaterial, 0.2, 0.2, 1);
        ClatterObjectData secondary = new ClatterObjectData(1, secondaryMaterial, 0.5, 0.1, 100);
        // Create the impact event.
        Impact impact = new Impact(primary, secondary, new Random());
        // Generate audio.
        impact.GetAudio(1);
        // Write the audio as a .wav file.
        WavWriter writer = new WavWriter("out.wav", overwrite: true);
        writer.Write(impact.samples.ToInt16Bytes());
        writer.End();
    }
}