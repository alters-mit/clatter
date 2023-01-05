namespace Clatter.Core
{
    /// <summary>
    /// Global values used across Clatter.
    /// </summary>
    public static class Globals
    {
        /// <summary>
        /// The default length of an array of audio samples.
        /// </summary>
        public const int DEFAULT_SAMPLES_LENGTH = 6000;


        /// <summary>
        /// The audio samples framerate.
        /// </summary>
        public static double framerate = 44100;
        /// <summary>
        /// The framerate expressed as an integer.
        /// </summary>
        public static int framerateInt = (int)framerate;
    }
}