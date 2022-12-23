In this example, we'll derive the mass of an object from its material and volume:

```csharp
using Clatter.Core;


public class Program
{
    private static void Main(string[] args)
    {
        float volume = 0.1f;
        // Start with a sized impact material.
        ImpactMaterialSized impactMaterialSized = ImpactMaterialSized.ceramic_2;
        // Convert to an impact material.
        ImpactMaterial impactMaterial = ImpactMaterialData.GetImpactMaterial(impactMaterialSized);
        // Get the density.
        float density = PhysicsValues.Density[impactMaterial];
        // Get the mass.
        float mass = density * volume;
        Console.WriteLine(mass);
    }
}
```