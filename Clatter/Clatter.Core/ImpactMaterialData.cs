using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;


namespace Clatter.Core
{
    /// <summary>
    /// Audio synthesis data for an impact material. Every ImpactMaterialData has a corresponding `ImpactMaterial` (see: ImpactMaterialData.impactMaterials).
    ///
    /// This class contains useful helper methods for converting `ImpactMaterialUnsized` values to `ImpactMaterial` values, as well as means of deriving density and "size bucket" values.
    ///
    /// ## Code Examples
    ///
    /// To derive the "size bucket" and mass from an `ImpactMaterialUnsized` value and volume:
    ///
    /// {code_example:SizeBucket}
    /// 
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
        /// RT60 values. The time it takes a mode power to decay 60dB (i.e. 10^-6) from its onset power in seconds.
        /// </summary>
        public double[] rt;
        /// <summary>
        /// A dictionary of impact data. Key = An ImpactMaterial value. Value = An ImpactMaterialData.
        /// </summary>
        public static Dictionary<ImpactMaterial, ImpactMaterialData> impactMaterials = new Dictionary<ImpactMaterial, ImpactMaterialData>();
        /// <summary>
        /// A dictionary of density values per impact material. Key = An ImpactMaterialUnsized. Value = Density in kg/m^3.
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
        /// <summary>
        /// Regex string to parse sized impact materials as unsized impact materials.
        /// </summary>
        private static readonly Regex SizedToUnSized = new Regex("(.*?)_([0-9])");


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
        /// Parse a size and an ImpactMaterialUnsized value to get an ImpactMaterial value.
        /// </summary>
        /// <param name="impactMaterialUnsized">The unsized impact material.</param>
        /// <param name="size">The size.</param>
        public static ImpactMaterial GetImpactMaterial(ImpactMaterialUnsized impactMaterialUnsized, int size)
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
        /// Returns the object's "size bucket" given the bounding box extents.
        /// </summary>
        /// <param name="extents">The object's bounding box extents.</param>
        public static int GetSize(Vector3d extents)
        {
            double s = extents.X + extents.Y + extents.Z;
            if (s <= 0.1)
            {
                return 0;
            }
            else if (s <= 0.2)
            {
                return 1;
            }
            else if (s <= 0.5)
            {
                return 2;
            }
            else if (s <= 1)
            {
                return 3;
            }
            else if (s <= 3)
            {
                return 4;
            }
            else
            {
                return 5;
            }
        }
        

        /// <summary>
        /// Returns the object's "size bucket" given its volume.
        /// </summary>
        /// <param name="volume">The object's volume.</param>
        public static int GetSize(double volume)
        {
            // Get the cubic root.
            double s = Math.Pow(volume, 1.0 / 3);
            return GetSize(new Vector3d(s, s, s));
        }


        /// <summary>
        /// Parse a sized impact material to get an un-sized impact material.
        /// </summary>
        /// <param name="impactMaterial">The sized impact material.</param>
        public static ImpactMaterialUnsized GetImpactMaterialUnsized(ImpactMaterial impactMaterial)
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