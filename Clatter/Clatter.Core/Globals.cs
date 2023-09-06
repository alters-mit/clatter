using System;
using ClatterRs;

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
        internal const int DEFAULT_SAMPLES_LENGTH = 6000;


        /// <summary>
        /// The audio samples framerate.
        /// </summary>
        public static int framerate = 44100;
        /// <summary>
        /// The audio samples framerate cast as a double.
        /// </summary>
        public static double framerateD =  framerate;
        /// <summary>
        /// If true, we tried loading the native Rust library.
        /// </summary>
        private static bool triedLoadingNativeLibrary;
        /// <summary>
        /// If true, we can use the native Rust library.
        /// </summary>
        private static bool canUseNativeLibrary;
        /// <summary>
        /// If true, we can use the native Rust library.
        /// </summary>
        internal static bool CanUseNativeLibrary
        {
            get
            {
                if (!triedLoadingNativeLibrary)
                {
                    triedLoadingNativeLibrary = true;
                    try
                    {
                        Ffi.is_ok();
                        canUseNativeLibrary = true;
                    }
                    catch (DllNotFoundException)
                    {
                        canUseNativeLibrary = false;
                    }
                }
                return canUseNativeLibrary;
            }
        }
    }
}