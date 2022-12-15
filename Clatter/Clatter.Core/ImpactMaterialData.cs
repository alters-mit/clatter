using System;
using System.IO;
using System.Collections.Generic;


namespace Clatter.Core
{
    /// <summary>
    /// Audio synthesis data for an impact material.
    /// </summary>
    public struct ImpactMaterialData
    {
        /// <summary>
        /// Mode properties.
        /// </summary>
        public double[] cf;
        /// <summary>
        /// Mode properties.
        /// </summary>
        public double[] op;
        /// <summary>
        /// Mode properties.
        /// </summary>
        public double[] rt;
        /// <summary>
        /// Impact data per material type.
        /// </summary>
        public static Dictionary<ImpactMaterialSized, ImpactMaterialData> impactMaterials = new Dictionary<ImpactMaterialSized, ImpactMaterialData>();


        /// <summary>
        /// Load impact material data.
        /// </summary>
        /// <param name="impactMaterial">The impact material.</param>
        /// <param name="raw">The raw byte data.</param>
        public static void Load(ImpactMaterialSized impactMaterial, byte[] raw)
        {
            // We already loaded the material.
            if (impactMaterials.ContainsKey(impactMaterial))
            {
                return;
            }
            // The first 12 bytes are the lengths of the arrays.
            double[] cf = new double[BitConverter.ToInt32(raw, 0)];
            double[] op = new double[BitConverter.ToInt32(raw, 4)];
            double[] rt = new double[BitConverter.ToInt32(raw, 8)];
            // Copy the data into the arrays.
            Buffer.BlockCopy(raw, 12, cf, 0, cf.Length * 8);
            Buffer.BlockCopy(raw, 12 + cf.Length * 8, op, 0, op.Length * 8);
            Buffer.BlockCopy(raw, 12 + cf.Length * 8 + op.Length * 8, rt, 0, rt.Length * 8);
            // Deserialize the material data from the JSON text.
            impactMaterials.Add(impactMaterial, new ImpactMaterialData()
            {
                cf = cf,
                op = op,
                rt = rt
            });
        }


        /// <summary>
        /// Load impact material data from a file relative to this assembly.
        /// </summary>
        /// <param name="impactMaterial">The impact material.</param>
        public static void Load(ImpactMaterialSized impactMaterial)
        {
            // We already loaded the material.
            if (impactMaterials.ContainsKey(impactMaterial))
            {
                return;
            }
            Load(impactMaterial, Loader.Load("ImpactMaterials." + impactMaterial + "_mm.bytes"));
        }
    }
}