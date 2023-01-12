using Clatter.Core;

public class SizeBucket
{
    private uint nextID;

    public AudioObjectData Get(ImpactMaterialUnsized impactMaterialUnsized, double amp, double resonance, double volume, ScrapeMaterial? scrapeMaterial)
    {
        // Get the "size bucket".
        int size = ImpactMaterialData.GetSize(volume);
        // Get the impact material.
        ImpactMaterial impactMaterial = ImpactMaterialData.GetImpactMaterial(impactMaterialUnsized, size);
        // Get the density of the material.
        double density = ImpactMaterialData.Density[impactMaterialUnsized];
        // Derive the mass.
        double mass = density * volume;
        // Create the object.
        AudioObjectData a = new AudioObjectData(nextID, impactMaterial, amp, resonance, mass, scrapeMaterial);
        // Increment the ID for the next object.
        nextID++;
        return a;
    }
}