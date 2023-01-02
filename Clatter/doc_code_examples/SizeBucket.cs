using Clatter.Core;

public class SizeBucket
{
    private uint nextID;

    public AudioObjectData Get(ImpactMaterialUnsized impactMaterialUnsized, double amp, double resonance, double volume, ScrapeMaterial? scrapeMaterial)
    {
        // Get the "size bucket".
        int size = AudioObjectData.GetSize(volume);
        // Get the impact material.
        ImpactMaterial impactMaterial = AudioObjectData.GetImpactMaterial(impactMaterialUnsized, size);
        // Get the density of the material.
        double density = PhysicsValues.Density[impactMaterialUnsized];
        // Derive the mass.
        double mass = density * volume;
        // Create the object.
        AudioObjectData a = new AudioObjectData(nextID, impactMaterial, amp, resonance, mass, scrapeMaterial);
        // Increment the ID for the next object.
        nextID++;
        return a;
    }
}