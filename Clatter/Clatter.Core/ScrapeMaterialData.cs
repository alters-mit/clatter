using System;
using System.IO;
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
            // Copy dsdx byte data to the array. The first four bytes of raw are the length of dsdx.
            double[] dsdx = new double[BitConverter.ToInt32(raw, 0)];
            Buffer.BlockCopy(raw, 16, dsdx, 0, dsdx.Length * 8);
            // Copy d2sdx2 byte data to the array. The second four bytes of raw are the length of d2sdx2.
            double[] d2sdx2 = new double[BitConverter.ToInt32(raw, 4)];
            Buffer.BlockCopy(raw, 16 + dsdx.Length * 8, d2sdx2, 0, d2sdx2.Length * 8);
            ScrapeMaterialData scrapeMaterialData = new ScrapeMaterialData
            {
                dsdx = dsdx,
                d2sdx2 = d2sdx2,
                roughnessGain = BitConverter.ToDouble(raw, 8)
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
            Load(scrapeMaterial, File.ReadAllBytes(Path.Combine(Paths.root, Paths.IMPACT_MATERIAL_FOLDER, scrapeMaterial + ".bytes")));
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