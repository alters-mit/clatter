﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;


namespace Clatter.Core
{
    /// <summary>
    /// Audio synthesis data for an impact material.
    /// </summary>
    public struct ImpactMaterialData
    {
        /// <summary>
        /// Regex string to parse sized impact materials as unsized impact materials.
        /// </summary>
        private static readonly Regex SizedToUnSized = new Regex("(.*?)_([0-9])");
        
        
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
        public static Dictionary<ImpactMaterial, ImpactMaterialData> impactMaterials = new Dictionary<ImpactMaterial, ImpactMaterialData>();


        /// <summary>
        /// Load impact material data from a file relative to this assembly.
        /// </summary>
        /// <param name="impactMaterial">The impact material.</param>
        public static void Load(ImpactMaterial impactMaterial)
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
        /// <param name="impactMaterialUnsized">The impact material.</param>
        /// <param name="size">The size.</param>
        public static ImpactMaterial GetImpactMaterialSized(ImpactMaterialUnsized impactMaterialUnsized, int size)
        {
            string m = impactMaterialUnsized + "_" + size;
            ImpactMaterial impactMaterial;
            if (!Enum.TryParse(m, out impactMaterial))
            {
                throw new Exception("Invalid impact material: " + m);
            }
            return impactMaterial;
        }


        /// <summary>
        /// Parse a sized impact material to get an un-sized impact material.
        /// </summary>
        /// <param name="impactMaterial">The sized impact material.</param>
        public static ImpactMaterialUnsized GetImpactMaterial(ImpactMaterial impactMaterial)
        {
            Match match = SizedToUnSized.Match(impactMaterial.ToString());
            if (match == null)
            {
                throw new Exception("Invalid ImpactMaterialSized: " + impactMaterial);
            }
            ImpactMaterialUnsized impactMaterialUnsized;
            if (!Enum.TryParse(match.Groups[1].Value, out impactMaterialUnsized))
            {
                throw new Exception("Invalid ImpactMaterialSized: " + impactMaterial);
            }
            return impactMaterialUnsized;
        }
    }
}