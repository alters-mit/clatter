﻿namespace Clatter.Core
{
    /// <summary>
    /// Audio data for a Clatter object.
    /// </summary>
    public class AudioObjectData
    {
        /// <summary>
        /// The unique ID of the object.
        /// </summary>
        public readonly uint id;
        /// <summary>
        /// The impact material.
        /// </summary>
        public readonly ImpactMaterialSized impactMaterial;
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
        public readonly float amp;
        /// <summary>
        /// The resonance value (0 to 1).
        /// </summary>
        public readonly float resonance;
        /// <summary>
        /// The mass of the object.
        /// </summary>
        public readonly float mass;
        /// <summary>
        /// The directional speed of the object.
        /// </summary>
        public float speed;
        /// <summary>
        /// The angular speed of the object.
        /// </summary>
        public float angularSpeed;
        /// <summary>
        /// The collision contacts area of a previous collision.
        /// </summary>
        public double previousArea = 0;
        /// <summary>
        /// If true, this is a previous area.
        /// </summary>
        public bool hasPreviousArea = false;
        
        
        /// <summary>
        /// (constructor)
        /// </summary>
        /// <param name="id">The ID of this object.</param>
        /// <param name="impactMaterial">The impact material.</param>
        /// <param name="amp">The audio amplitude (0 to 1).</param>
        /// <param name="resonance">The resonance value (0 to 1).</param>
        /// <param name="mass">The mass of the object.</param>
        /// <param name="scrapeMaterial">The scrape material. Can be null.</param>
        public AudioObjectData(uint id, ImpactMaterialSized impactMaterial, float amp, float resonance, float mass, ScrapeMaterial? scrapeMaterial = null)
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
            this.amp = amp;
            this.resonance = resonance;
            this.mass = mass;
        }
    }
}