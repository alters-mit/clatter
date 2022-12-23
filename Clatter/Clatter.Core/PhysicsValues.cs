using System.Collections.Generic;


namespace Clatter.Core
{
    /// <summary>
    /// Physical values for materials.
    /// </summary>
    public static class PhysicsValues
    {
        /// <summary>
        /// Density in kg/m^3 per ImpactMaterial.
        /// </summary>
        public static readonly Dictionary<ImpactMaterial, int> Density = new Dictionary<ImpactMaterial, int>()
        {
            { ImpactMaterial.ceramic, 2180 },
            { ImpactMaterial.glass, 2500 },
            { ImpactMaterial.stone, 2000 },
            { ImpactMaterial.metal, 8450 },
            { ImpactMaterial.wood_hard, 1200 },
            { ImpactMaterial.wood_medium, 700 },
            { ImpactMaterial.wood_soft, 400 },
            { ImpactMaterial.fabric, 1540 },
            { ImpactMaterial.leather, 860 },
            { ImpactMaterial.plastic_hard, 1150 },
            { ImpactMaterial.plastic_soft_foam, 285 },
            { ImpactMaterial.rubber, 1522 },
            { ImpactMaterial.paper, 1200 },
            { ImpactMaterial.cardboard, 698 }
        };
    }
}