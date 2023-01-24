namespace Clatter.Unity
{
    /// <summary>
    /// The mode for setting the mass of an `ClatterObject`.
    /// </summary>
    public enum MassMode : byte
    {
        /// <summary>
        /// The mass is the same as the mass of the object's Rigidbody/ArticulationBody.
        /// </summary>
        body = 1,
        /// <summary>
        /// The mass is set explicitly in `ClatterObject` and may vary from the mass of the object's Rigidbody/ArticulationBody. This is useful if you want to create unrealistic sounds.
        /// </summary>
        fake_mass = 2,
        /// <summary>
        /// The mass is derived from the object's impact material, volume, and ClatterObject.hollowness.
        /// </summary>
        volume = 4
    }
}