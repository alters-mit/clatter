using UnityEngine;
using Clatter.Core;


namespace Clatter.Unity
{
    /// <summary>
    /// Load audio materials from Resources.
    /// </summary>
    public static class MaterialLoader
    {
        /// <summary>
        /// Load an impact material from Resources.
        /// </summary>
        /// <param name="impactMaterial">The impact material.</param>
        public static void Load(ImpactMaterialSized impactMaterial)
        {
            ImpactMaterialData.Load(impactMaterial, Resources.Load<TextAsset>(Paths.IMPACT_MATERIAL_FOLDER + impactMaterial.ToString() + "_mm").bytes);
        }
        
        
        /// <summary>
        /// Load a scrape material from Resources.
        /// </summary>
        /// <param name="scrapeMaterial">The scrape material.</param>
        public static void Load(ScrapeMaterial scrapeMaterial)
        {
            ScrapeMaterialData.Load(scrapeMaterial, Resources.Load<TextAsset>(Paths.SCRAPE_MATERIAL_DATA_FOLDER + scrapeMaterial.ToString()).bytes);
        }
    }
}