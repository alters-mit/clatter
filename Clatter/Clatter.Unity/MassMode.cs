namespace Clatter.Unity
{
    /// <summary>
    /// The mode for setting the mass of an `AudioProducingObject`.
    ///
    /// - body: The mass is the same as the mass of the object's Rigidbody/ArticulationBody.
    /// - fake_mass: The mass is set explicitly in `AudioProducingObject` and may vary from the mass of the object's Rigidbody/ArticulationBody. This is useful if you want to create unrealistic sounds.
    /// - volume: The mass is derived from the object's volume and AudioProducingObject.hollowness.
    /// </summary>
    public enum MassMode : byte
    {
        body = 1,
        fake_mass = 2,
        volume = 4
    }
}