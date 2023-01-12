using Clatter.Core;

public class ImpactMaterialToScrapeMaterial
{
    public static ScrapeMaterial GetScrapeMaterial(ImpactMaterialUnsized impactMaterialUnsized)
    {
        return ScrapeMaterialData.ImpactMaterialUnsizedToScrapeMaterial[impactMaterialUnsized];
    }


    public static ScrapeMaterial GetScrapeMaterial(ImpactMaterial impactMaterial)
    {
        ImpactMaterialUnsized impactMaterialUnsized = ImpactMaterialData.GetImpactMaterialUnsized(impactMaterial);
        return GetScrapeMaterial(impactMaterialUnsized);
    }
}