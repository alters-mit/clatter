namespace Clatter.Unity
{
    /// <summary>
    /// The mode for setting the mass of an `AudioProducingObject`.
    /// </summary>
    public enum MassMode : byte
    {
        /// <summary>
        /// The mass is the same as the mass of the object's Rigidbody/ArticulationBody.
        /// </summary>
        body = 1,
        /// <summary>
        /// The mass is set explicitly in `AudioProducingObject` and may vary from the mass of the object's Rigidbody/ArticulationBody. This is useful if you want to create unrealistic sounds.
        /// </summary>
        fake_mass = 2,
        /// <summary>
        /// The mass is derived from the object's impact material, volume, and AudioProducingObject.hollowness.
        /// </summary>
        volume = 4
    }
}