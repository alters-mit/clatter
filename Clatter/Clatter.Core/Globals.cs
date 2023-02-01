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
        public static int framerate = 44100;
        /// <summary>
        /// The audio samples framerate cast as a double.
        /// </summary>
        public static double framerateD =  framerate;
    }
}