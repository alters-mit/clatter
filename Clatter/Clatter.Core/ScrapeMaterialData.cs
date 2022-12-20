using System;
using System.Collections.Generic;


namespace Clatter.Core
{
    /// <summary>
    /// Audio synthesis data for an scrape material.
    /// </summary>
    public struct ScrapeMaterialData
    {
        /// <summary>
        /// Meters per pixel on the scrape surface.
        /// </summary>
        public const double SCRAPE_M_PER_PIXEL = 1394.068 * 10e-10;

        
        /// <summary>
        /// First-derivative data.
        /// </summary>
        public double[] dsdx;
        /// <summary>
        /// Secondary-derivative data.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public double[] d2sdx2;
        /// <summary>
        /// The roughness volume gain.
        /// </summary>
        public double roughnessGain;
        /// <summary>
        /// Scrape data per material type.
        /// </summary>
        private static readonly Dictionary<ScrapeMaterial, ScrapeMaterialData> ScrapeMaterials = new Dictionary<ScrapeMaterial, ScrapeMaterialData>();



        /// <summary>
        /// Load the scrape material data into memory.
        /// </summary>
        /// <param name="scrapeMaterial">The scrape material.</param>
        /// <param name="raw">The raw byte data loaded from a file.</param>
        public static void Load(ScrapeMaterial scrapeMaterial, byte[] raw)
        {
            // Use a pre-loaded material.
            if (ScrapeMaterials.ContainsKey(scrapeMaterial))
            {
                return;
            }
            // Get the surface array.
            double[] surface = new double[(raw.Length - 8) / 8];
            Buffer.BlockCopy(raw, 8, surface, 0, raw.Length - 8);
            // Get first-derivative data.
            double[] dsdx = new double[surface.Length - 1];
            for (int i = 1; i < surface.Length; i++)
            {
                dsdx[i - 1] = (surface[i] - surface[i - 1]) / SCRAPE_M_PER_PIXEL;
            }
            // Get second-derivative data.
            // ReSharper disable once InconsistentNaming
            double[] d2sdx2 = new double[dsdx.Length - 1];
            for (int i = 1; i < dsdx.Length; i++)
            {
                d2sdx2[i - 1] = (dsdx[i] - dsdx[i - 1]) / SCRAPE_M_PER_PIXEL;
            }
            ScrapeMaterialData scrapeMaterialData = new ScrapeMaterialData
            {
                dsdx = dsdx,
                d2sdx2 = d2sdx2,
                roughnessGain = BitConverter.ToDouble(raw, 0)
            };
            ScrapeMaterials.Add(scrapeMaterial, scrapeMaterialData);
        }


        /// <summary>
        /// Load scrape material data from a file relative to this assembly.
        /// </summary>
        /// <param name="scrapeMaterial">The scrape material.</param>
        public static void Load(ScrapeMaterial scrapeMaterial)
        {
            // We already loaded the material.
            if (ScrapeMaterials.ContainsKey(scrapeMaterial))
            {
                return;
            }
            Load(scrapeMaterial, Loader.Load("ScrapeMaterials." + scrapeMaterial + ".bytes"));
        }


        /// <summary>
        /// Returns the data associated with the scrape material.
        /// </summary>
        /// <param name="scrapeMaterial">The scrape material.</param>
        public static ScrapeMaterialData Get(ScrapeMaterial scrapeMaterial)
        {
            return ScrapeMaterials[scrapeMaterial];
        }
    }
}