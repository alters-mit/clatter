﻿using System;


namespace Clatter.Core
{
    /// <summary>
    /// A cached array slice of audio samples.
    /// </summary>
    public class Samples
    {
        /// <summary>
        /// The audio samples.
        /// </summary>
        private double[] samples = new double[Globals.DEFAULT_SAMPLES_LENGTH];
        /// <summary>
        /// The length of the samples.
        /// </summary>
        private int length;


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
            if (length >= this.samples.Length)
            {
                Array.Resize(ref this.samples, length * 2);
            }
            // Set the length of the audio samples.
            this.length = length;
            // Copy the samples.
            Buffer.BlockCopy(samples, start * 8, this.samples, 0, length * 8);
        }


        /// <summary>
        /// Returns the samples as floats.
        /// </summary>
        public float[] ToFloats()
        {
            return samples.ToFloats(length);
        }


        /// <summary>
        /// Returns the samples as an int16 byte array.
        /// </summary>
        public byte[] ToInt16Bytes()
        {
            return samples.ToInt16Bytes(length);
        }
    }
}