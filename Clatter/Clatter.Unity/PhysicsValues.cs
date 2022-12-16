using System.Collections.Generic;
using Clatter.Core;


namespace Clatter.Unity
{
    /// <summary>
    /// Physical values per impact material.
    /// Note that bounciness is not a dictionary because typically this should vary between objects even if they have the same ImpactMaterial.
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
        /// <summary>
        /// Unity physic material dynamic friction values per ImpactMaterial.
        /// </summary>
        public static readonly Dictionary<ImpactMaterial, float> DynamicFriction = new Dictionary<ImpactMaterial, float>()
        {
            { ImpactMaterial.ceramic, 0.47f },
            { ImpactMaterial.wood_hard, 0.35f },
            { ImpactMaterial.wood_medium, 0.35f },
            { ImpactMaterial.wood_soft, 0.35f },
            { ImpactMaterial.cardboard, 0.45f },
            { ImpactMaterial.paper, 0.47f },
            { ImpactMaterial.glass, 0.65f },
            { ImpactMaterial.fabric, 0.65f },
            { ImpactMaterial.leather, 0.4f },
            { ImpactMaterial.stone, 0.7f },
            { ImpactMaterial.rubber, 0.75f },
            { ImpactMaterial.plastic_hard, 0.3f },
            { ImpactMaterial.plastic_soft_foam, 0.45f },
            { ImpactMaterial.metal, 0.43f }
        };
        /// <summary>
        /// Unity physic material static friction values per ImpactMaterial.
        /// </summary>
        public static readonly Dictionary<ImpactMaterial, float> StaticFriction = new Dictionary<ImpactMaterial, float>
        {
            { ImpactMaterial.ceramic, 0.47f },
            { ImpactMaterial.wood_hard, 0.37f },
            { ImpactMaterial.wood_medium, 0.37f },
            { ImpactMaterial.wood_soft, 0.37f },
            { ImpactMaterial.cardboard, 0.48f },
            { ImpactMaterial.paper, 0.5f },
            { ImpactMaterial.glass, 0.68f },
            { ImpactMaterial.fabric, 0.67f },
            { ImpactMaterial.leather, 0.43f },
            { ImpactMaterial.stone, 0.72f },
            { ImpactMaterial.rubber, 0.8f },
            { ImpactMaterial.plastic_hard, 0.35f },
            { ImpactMaterial.plastic_soft_foam, 0.47f },
            { ImpactMaterial.metal, 0.47f }
        };
    }
}