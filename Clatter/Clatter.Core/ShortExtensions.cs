namespace Clatter.Core
{
    /// <summary>
    /// Extensions for shorts.
    /// </summary>
    public static class ShortExtensions
    {
        /// <summary>
        /// Fills an existing byte array with bytes.
        /// </summary>
        /// <param name="value">(this)</param>
        /// <param name="bytes">The byte array.</param>
        public static unsafe void GetBytes(this short value, byte[] bytes)
        {
            fixed (byte* numPtr = bytes)
                *(short*) numPtr = value;
        }
    }
}