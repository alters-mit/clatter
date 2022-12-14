using UnityEngine;
using Clatter.Core;


namespace Clatter.Unity
{
    /// <summary>
    /// Vector3d extension methods.
    /// </summary>
    public static class Vector3dExtensions
    {
        /// <summary>
        /// Returns a Unity Vector3.
        /// </summary>
        /// <param name="vector">(this)</param>
        public static Vector3 ToVector3(this Vector3d vector)
        {
            return new Vector3((float)vector.x, (float)vector.y, (float)vector.z);
        }
    }
}