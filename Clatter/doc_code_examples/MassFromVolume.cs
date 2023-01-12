using Clatter.Core;

public class MassFromVolume
{
    private uint nextID;

    public AudioObjectData Get(ImpactMaterial impactMaterial, double amp, double resonance, double volume, ScrapeMaterial? scrapeMaterial)
    {
        // Get the "unsized" impact material.
        ImpactMaterialUnsized impactMaterialUnsized = ImpactMaterialData.GetImpactMaterialUnsized(impactMaterial);
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