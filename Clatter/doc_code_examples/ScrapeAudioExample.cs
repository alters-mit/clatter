﻿using System;
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
        ClatterObjectData primary = new ClatterObjectData(0, primaryMaterial, 0.2, 0.2, 1);
        ClatterObjectData secondary = new ClatterObjectData(1, secondaryMaterial, 0.5, 0.1, 100, scrapeMaterial);
        // Initialize the scrape.
        Scrape scrape = new Scrape(scrapeMaterial, primary, secondary, new Random());
        // Start writing audio.
        WavWriter writer = new WavWriter("out.wav", overwrite: true);
        // Generate the scrape.
        int count = Scrape.GetNumScrapeEvents(0.5);
        for (int i = 0; i < count; i++)
        {
            scrape.GetAudio(1);
            writer.Write(scrape.samples.ToInt16Bytes());
        }
        writer.End();
    }
}