namespace Clatter.Core
{
    /// <summary>
    /// Audio materials for scrape sounds. Scrape materials are always un-sized, unlike `ImpactMaterial` values.
    /// 
    /// Due to separate recording processes, scrape materials don't have the same names as impact materials and don't map exactly to `ImpactMaterialUnsized` values. See: ScrapeMaterialData.ImpactMaterialUnsizedToScrapeMaterial:
    ///
    /// {code_example:ImpactMaterialToScrapeMaterial}
    /// 
    /// </summary>
    public enum ScrapeMaterial : byte
    {
        plywood = 0,
        ceramic = 1,
        pvc = 2,
        rough_wood = 3,
        acrylic = 4,
        sanded_acrylic = 5,
        vinyl = 6,
        poplar_wood = 7,
        bass_wood = 8,
        polycarbonate = 9,
        polyethylene = 10,
        sandpaper = 11
    }
}