namespace Clatter.Unity
{
    /// <summary>
    /// The mode for setting the physic material (dynamic friction, static friction, bounciness) of a `ClatterObject`.
    /// </summary>
    public enum PhysicMaterialMode : byte
    {
        /// <summary>
        /// Don't set the object's physic material.
        /// </summary>
        none = 1,
        /// <summary>
        /// Automatically set the object's dynamic friction and static friction and manually set the object's bounciness.
        /// </summary>
        auto = 2,
        /// <summary>
        /// Manually set the object's dynamic friction, static friction, and bounciness.
        /// </summary>
        manual = 4
    }
}