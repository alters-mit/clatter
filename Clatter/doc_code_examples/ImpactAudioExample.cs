using Clatter.Core;


public class Program
{
    private static void Main(string[] args)
    {
        // Define the two objects.
        AudioObjectData primary = new AudioObjectData(0, ImpactMaterial.glass_1, 0.2, 0.2, 1);
        AudioObjectData secondary = new AudioObjectData(1, ImpactMaterial.stone_4, 0.5, 0.1, 100);
        // Create a Random object.
        Random rng = new Random();
        // Create a new impact event series.
        Impact impact = new Impact(primary, secondary, rng);
        // Define an output directory.
        string outputDirectory = Path.GetFullPath("output");
        double speed = 1.5;
        double deceleration = 0.1;
        // Create five impact sounds and save them to disk.
        for (int i = 0; i < 5; i++)
        {
            // Generate audio.
            impact.GetAudio(speed, rng);
            // Write a .wav file.
            WavWriter writer = new WavWriter(Path.Combine(outputDirectory, i + ".wav"));
            writer.Write(impact.samples.ToInt16Bytes());
            writer.End();
            // Decelerate.
            speed -= deceleration;
        }
    }
}