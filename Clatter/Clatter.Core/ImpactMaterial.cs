namespace Clatter.Core
{
    /// <summary>
    /// Audio materials for impact sounds without size bucket information. See: ImpactMaterialSized.
    /// </summary>
    public enum ImpactMaterial : byte
    {
        ceramic = 0,
        glass = 1,
        metal = 2,
        wood_hard = 3,
        wood_medium = 4,
        wood_soft = 5,
        cardboard = 6,
        paper = 7,
        plastic_hard = 8,
        plastic_soft_foam = 9,
        rubber = 10,
        fabric = 11,
        leather = 12,
        stone = 13
    }
}