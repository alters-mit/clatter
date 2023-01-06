using System;


namespace Clatter.Core
{
    /// <summary>
    /// This is thrown when an `AudioGenerator` waits too long for its audio generation threads to finish.
    /// </summary>
    public class AudioGeneratorTimeoutException : Exception
    {
        /// <summary>
        /// (constructor)
        /// </summary>
        /// <param name="numIterations">The number of thread iterations.</param>
        public AudioGeneratorTimeoutException(uint numIterations)
            : base("Too many thread iterations: " + numIterations)
        {
        }
    }
}