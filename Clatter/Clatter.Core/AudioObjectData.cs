﻿namespace Clatter.Core
{
    /// <summary>
    /// Audio data for a Clatter object.
    ///
    /// Audio generation in Clatter is always the result of a collision event between two AudioObjectData objects. Each object in your scene or simulation must have corresponding AudioObjectData.
    ///
    /// In many cases, it is possible to derive the AudioObjectData constructor parameters from other physical values. See: `ImpactMaterialData`.
    ///
    /// ## Code Examples
    /// 
    /// This is a minimal example of how to instantiate an AudioObjectData:
    ///
    /// {code_example:AudioObjectDataConstructor}
    ///
    /// To generate scrape audio, the object acting as the "scrape surface" must have a `ScrapeMaterial`. For example, if you want to scrape a block along a table, the table needs a `ScrapeMaterial` (and the block doesn't). This is a minimal example of how to set an object's `ScrapeMaterial`:
    ///
    /// {code_example:AudioObjectDataConstructorScrapeMaterial}
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
        /// The mass of the object in kilograms.
        /// </summary>
        public readonly double mass;
        /// <summary>
        /// The directional speed of the object in meters per second.
        /// </summary>
        public double speed;
        /// <summary>
        /// The angular speed of the object  in meters per second.
        /// </summary>
        public double angularSpeed;


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
    }
}