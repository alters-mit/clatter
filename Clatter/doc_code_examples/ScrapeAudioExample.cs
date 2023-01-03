using Clatter.Core;


public class ScrapeAudioExample
{
    private static void Main(string[] args)
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
        // Start writing audio.
        WavWriter writer = new WavWriter("out.wav", overwrite: true);
        // Generate the scrape.
        int count = Scrape.GetNumScrapeEvents(0.5);
        for (int i = 0; i < count; i++)
        {
            scrape.GetAudio(1, rng);
            writer.Write(scrape.samples.ToInt16Bytes());
        }
        writer.End();
    }
}