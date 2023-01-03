using System.Collections.Generic;
using Clatter.Core;


namespace Clatter.Unity
{
    /// <summary>
    /// Physical values per impact material.
    ///
    /// Note that the keys of each dictionary are `Clatter.Core.ImpactMaterialSized` values. See: Clatter.Core.AudioObjectData.GetImpactMaterialUnsized(impactMaterial).
    ///
    /// Bounciness is included here because typically it varies between objects even if they have the same impact material.
    /// </summary>
    public static class PhysicMaterialValues
    {
        /// <summary>
        /// Unity physic material dynamic friction values per ImpactMaterial.
        /// </summary>
        public static readonly Dictionary<ImpactMaterialUnsized, float> DynamicFriction = new Dictionary<ImpactMaterialUnsized, float>()
        {
            { ImpactMaterialUnsized.ceramic, 0.47f },
            { ImpactMaterialUnsized.wood_hard, 0.35f },
            { ImpactMaterialUnsized.wood_medium, 0.35f },
            { ImpactMaterialUnsized.wood_soft, 0.35f },
            { ImpactMaterialUnsized.cardboard, 0.45f },
            { ImpactMaterialUnsized.paper, 0.47f },
            { ImpactMaterialUnsized.glass, 0.65f },
            { ImpactMaterialUnsized.fabric, 0.65f },
            { ImpactMaterialUnsized.leather, 0.4f },
            { ImpactMaterialUnsized.stone, 0.7f },
            { ImpactMaterialUnsized.rubber, 0.75f },
            { ImpactMaterialUnsized.plastic_hard, 0.3f },
            { ImpactMaterialUnsized.plastic_soft_foam, 0.45f },
            { ImpactMaterialUnsized.metal, 0.43f }
        };
        /// <summary>
        /// Unity physic material static friction values per ImpactMaterial.
        /// </summary>
        public static readonly Dictionary<ImpactMaterialUnsized, float> StaticFriction = new Dictionary<ImpactMaterialUnsized, float>
        {
            { ImpactMaterialUnsized.ceramic, 0.47f },
            { ImpactMaterialUnsized.wood_hard, 0.37f },
            { ImpactMaterialUnsized.wood_medium, 0.37f },
            { ImpactMaterialUnsized.wood_soft, 0.37f },
            { ImpactMaterialUnsized.cardboard, 0.48f },
            { ImpactMaterialUnsized.paper, 0.5f },
            { ImpactMaterialUnsized.glass, 0.68f },
            { ImpactMaterialUnsized.fabric, 0.67f },
            { ImpactMaterialUnsized.leather, 0.43f },
            { ImpactMaterialUnsized.stone, 0.72f },
            { ImpactMaterialUnsized.rubber, 0.8f },
            { ImpactMaterialUnsized.plastic_hard, 0.35f },
            { ImpactMaterialUnsized.plastic_soft_foam, 0.47f },
            { ImpactMaterialUnsized.metal, 0.47f }
        };
    }
}