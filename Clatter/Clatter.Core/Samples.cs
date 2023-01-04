namespace Clatter.Core
{
    /// <summary>
    /// Samples caches an array of audio data as doubles to speed up data caching and minimize memory allocation.
    ///
    /// See: `Impact` and `Scrape`, both of which have a samples field.
    /// </summary>
    public class Samples
    {
        /// <summary>
        /// The audio samples.
        /// </summary>
        public double[] samples = new double[Globals.DEFAULT_SAMPLES_LENGTH];
        /// <summary>
        /// The length of the samples.
        /// </summary>
        public int length;
        

        /// <summary>
        /// Returns the samples as floats. Use this in Unity, which accepts an array of floats for AudioClip data.
        /// </summary>
        public float[] ToFloats()
        {
            return samples.ToFloats(length);
        }


        /// <summary>
        /// Returns the samples as an int16 byte array. Use this to write out valid .wav file data.
        /// </summary>
        public byte[] ToInt16Bytes()
        {
            return samples.ToInt16Bytes(length);
        }
    }
}