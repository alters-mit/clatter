using System;


namespace Clatter.Core
{
    /// <summary>
    /// A cached array slice of audio samples.
    /// </summary>
    public class Samples
    {
        /// <summary>
        /// The default length of the samples array.
        /// </summary>
        private const int DEFAULT_LENGTH = 6000;


        /// <summary>
        /// The audio samples.
        /// </summary>
        private double[] _samples = new double[DEFAULT_LENGTH];
        /// <summary>
        /// The length of the samples.
        /// </summary>
        private int _length;


        /// <summary>
        /// Set the audio samples.
        /// Cast from an array of doubles to an array of floats and fill all channels.
        /// </summary>
        /// <param name="samples">The raw samples as a single-channel array of doubles.</param>
        /// <param name="start">The start index in the raw doubles samples array.</param>
        /// <param name="length">The length of my samples array.</param>
        public void Set(double[] samples, int start, int length)
        {
            // Resize the samples if needed.
            if (length >= _samples.Length)
            {
                Array.Resize(ref this._samples, length * 2);
            }
            // Set the length of the audio samples.
            this._length = length;
            // Copy the samples.
            Buffer.BlockCopy(samples, start * 8, this._samples, 0, length * 8);
        }


        /// <summary>
        /// Copy these samples to another samples.
        /// </summary>
        /// <param name="b">The other samples.</param>
        public void CopyTo(Samples b)
        {
            if (b._samples.Length < _samples.Length)
            {
                Array.Resize(ref b._samples, _samples.Length);
            }
            Buffer.BlockCopy(_samples, 0, b._samples, 0, _length * 8);
            b._length = _length;
        }


        /// <summary>
        /// Returns the samples as floats.
        /// </summary>
        public float[] ToFloats()
        {
            return _samples.ToFloats(_length);
        }


        /// <summary>
        /// Returns the samples as an int16 byte array.
        /// </summary>
        public byte[] ToInt16Bytes()
        {
            return _samples.ToInt16Bytes(_length);
        }
    }
}