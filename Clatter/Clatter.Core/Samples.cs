namespace Clatter.Core
{
    /// <summary>
    /// Cached audio data as an array of doubles that can be converted to an array of floats (for usage in Unity) or a byte array of int16 data (for .wav files).
    ///
    /// To avoid unnecessary memory allocations, the samples array is usually longer than the "actual" data size. Always use Samples.length instead of Samples.samples.Length to evaluate the size of the array.
    ///
    /// See: `Impact` and `Scrape`, both of which have a samples field.
    /// </summary>
    public class Samples
    {
        /// <summary>
        /// The audio samples.
        /// </summary>
        public double[] samples;
        /// <summary>
        /// The true length of the samples data; this is usually less than samples.Length.
        /// </summary>
        public int length;


        /// <summary>
        /// (constructor)
        /// </summary>
        /// <param name="size">The initial size of the array. This doesn't set the length field.</param>
        public Samples(int size)
        {
            samples = new double[size];
        }
        

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