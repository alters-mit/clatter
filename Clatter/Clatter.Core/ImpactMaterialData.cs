using System;
using System.Collections.Generic;


namespace Clatter.Core
{
    /// <summary>
    /// Audio synthesis data for an impact material.
    /// </summary>
    public struct ImpactMaterialData
    {
        /// <summary>
        /// The frequency of the sinusoid used to create the mode in Hz.
        /// </summary>
        public double[] cf;
        /// <summary>
        /// The power of the mode at the onset, in dB relative to the amplitude of the "onset click".
        /// </summary>
        public double[] op;
        /// <summary>
        /// RT60 values. The time it takes a mode power to decay 60dB (i.e. 10**(-6)) from its onset power in seconds.
        /// </summary>
        public double[] rt;
        /// <summary>
        /// Impact data per material type.
        /// </summary>
        public static Dictionary<ImpactMaterialSized, ImpactMaterialData> impactMaterials = new Dictionary<ImpactMaterialSized, ImpactMaterialData>();


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
            // Load the raw byte data.
            byte[] raw = Loader.Load("ImpactMaterials." + impactMaterial + "_mm.bytes");
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
        /// Parse a size and an impact material to get an ImpactMaterialSized value.
        /// </summary>
        /// <param name="impactMaterial">The impact material.</param>
        /// <param name="size">The size.</param>
        public static ImpactMaterialSized GetImpactMaterialSized(ImpactMaterial impactMaterial, int size)
        {
            string m = impactMaterial + "_" + size;
            ImpactMaterialSized impactMaterialSized;
            if (!Enum.TryParse(m, out impactMaterialSized))
            {
                throw new Exception("Invalid impact material: " + m);
            }
            return impactMaterialSized;
        }
    }
}