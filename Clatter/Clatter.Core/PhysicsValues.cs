using System.Collections.Generic;


namespace Clatter.Core
{
    /// <summary>
    /// Physical values for materials.
    /// </summary>
    public static class PhysicsValues
    {
        /// <summary>
        /// Density in kg/m^3 per ImpactMaterialUnsized.
        /// </summary>
        public static readonly Dictionary<ImpactMaterialUnsized, int> Density = new Dictionary<ImpactMaterialUnsized, int>()
        {
            { ImpactMaterialUnsized.ceramic, 2180 },
            { ImpactMaterialUnsized.glass, 2500 },
            { ImpactMaterialUnsized.stone, 2000 },
            { ImpactMaterialUnsized.metal, 8450 },
            { ImpactMaterialUnsized.wood_hard, 1200 },
            { ImpactMaterialUnsized.wood_medium, 700 },
            { ImpactMaterialUnsized.wood_soft, 400 },
            { ImpactMaterialUnsized.fabric, 1540 },
            { ImpactMaterialUnsized.leather, 860 },
            { ImpactMaterialUnsized.plastic_hard, 1150 },
            { ImpactMaterialUnsized.plastic_soft_foam, 285 },
            { ImpactMaterialUnsized.rubber, 1522 },
            { ImpactMaterialUnsized.paper, 1200 },
            { ImpactMaterialUnsized.cardboard, 698 }
        };
    }
}