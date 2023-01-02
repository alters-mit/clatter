using System;
using System.Text.RegularExpressions;


namespace Clatter.Core
{
    /// <summary>
    /// Audio data for a Clatter object.
    ///
    /// Audio generation in Clatter is always the result of a collision event between two AudioObjectData objects. Each object in your scene or simulation must have corresponding AudioObjectData.
    ///
    /// This is a minimal example of how to instantiate an AudioObjectData:
    ///
    /// {code_example:AudioObjectDataConstructor}
    ///
    /// You can optionally set a Clatter object as a "scrape surface" by setting the scrapeMaterial constructor parameter:
    ///
    /// {code_example:AudioObjectDataConstructorScrapeMaterial}
    ///
    /// In many cases, it is possible to derive the AudioObjectData constructor parameters from other physical values:
    ///
    /// **Example A.** Derive mass from volume. In this example, we need to convert the `ImpactMaterial` to an `ImpactMaterialUnsized` in order to look up the density:
    ///
    /// {code_example:MassFromVolume}
    ///
    /// **Example B.** Derive the "size bucket" from an `ImpactMaterialUnsized` value and volume:
    ///
    /// {code_example:SizeBucket}
    /// 
    /// </summary>
    public class AudioObjectData
    {
        /// <summary>
        /// The ID of the object. This must always be unique.
        /// </summary>
        public readonly uint id;
        /// <summary>
        /// The impact material.
        /// </summary>
        public readonly ImpactMaterial impactMaterial;
        /// <summary>
        /// The scrape material.
        /// </summary>
        public readonly ScrapeMaterial scrapeMaterial;
        /// <summary>
        /// If true, this object has a scrape material.
        /// </summary>
        public readonly bool hasScrapeMaterial;
        /// <summary>
        /// The audio amplitude (0 to 1).
        /// </summary>
        public readonly double amp;
        /// <summary>
        /// The resonance value (0 to 1).
        /// </summary>
        public readonly double resonance;
        /// <summary>
        /// The mass of the object.
        /// </summary>
        public readonly double mass;
        /// <summary>
        /// The directional speed of the object.
        /// </summary>
        public double speed;
        /// <summary>
        /// The angular speed of the object.
        /// </summary>
        public double angularSpeed;
        /// <summary>
        /// The collision contacts area of a previous collision.
        /// </summary>
        public double previousArea = 0;
        /// <summary>
        /// If true, this object has contacted another object previously and generated contact area.
        /// </summary>
        public bool hasPreviousArea = false;
        /// <summary>
        /// Regex string to parse sized impact materials as unsized impact materials.
        /// </summary>
        private static readonly Regex SizedToUnSized = new Regex("(.*?)_([0-9])");
        
        
        /// <summary>
        /// (constructor)
        /// </summary>
        /// <param name="id">The ID of this object.</param>
        /// <param name="impactMaterial">The impact material.</param>
        /// <param name="amp">The audio amplitude (0 to 1).</param>
        /// <param name="resonance">The resonance value (0 to 1).</param>
        /// <param name="mass">The mass of the object.</param>
        /// <param name="scrapeMaterial">The scrape material. Can be null.</param>
        public AudioObjectData(uint id, ImpactMaterial impactMaterial, double amp, double resonance, double mass, ScrapeMaterial? scrapeMaterial = null)
        {
            this.id = id;
            this.impactMaterial = impactMaterial;
            // Set the scrape material.
            hasScrapeMaterial = scrapeMaterial != null;
            if (hasScrapeMaterial)
            {
                // ReSharper disable once PossibleInvalidOperationException
                this.scrapeMaterial = (ScrapeMaterial)scrapeMaterial;
            }
            else
            {
                this.scrapeMaterial = default;
            }
            // Set the physics parameters.
            this.amp = amp.Clamp(0, 1);
            this.resonance = resonance.Clamp(0, 1);
            this.mass = mass;
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
        /// Returns the object's "size bucket" given its volume..
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