using System;
using System.Collections.Generic;


namespace Clatter.Core
{
    /// <summary>
    /// Median-Filters are non-linear filters, returning
    /// the median of a sample window as output. Median-Filters
    /// perform well for de-noise applications where it's
    /// important to not loose sharp steps/edges.
    /// This is an optimized version of this class: https://github.com/mathnet/mathnet-filtering/blob/master/src/Filtering/Median/OnlineMedianFilter.cs
    /// </summary>
    public class MedianFilter
    {
        /// <summary>
        /// The filter buffer.
        /// </summary>
        private readonly double[] buffer;
        /// <summary>
        /// A dictionary of cached offset buffers. Key = The length of the buffer.
        /// </summary>
        private readonly Dictionary<int, double[]> offsetBuffers = new Dictionary<int, double[]>();
        /// <summary>
        /// The current offset.
        /// </summary>
        private int offset;
        /// <summary>
        /// If true, the buffer is full.
        /// </summary>
        private bool bufferFull;
        

        /// <summary>
        /// Create a Median Filter.
        /// </summary>
        public MedianFilter(int windowSize)
        {
            // Set the buffer.
            buffer = new double[windowSize];
            // Generate offset buffers.
            for (int i = windowSize - 1; i >= 0; i--)
            {
                offsetBuffers.Add(i, new double[i]);
            }
        }

        /// <summary>
        /// Process a single sample.
        /// </summary>
        public double ProcessSample(double sample)
        {
            buffer[offset = (offset == 0) ? buffer.Length - 1 : offset - 1] = sample;
            bufferFull |= offset == 0;
            if (bufferFull)
            {
                return buffer.MedianInplace();
            }
            else
            {
                int length = buffer.Length - offset;
                // Copy to the offset buffer.
                Buffer.BlockCopy(buffer, offset * 8, offsetBuffers[length], 0, offsetBuffers[length].Length * 8);
                return offsetBuffers[length].MedianInplace();
            }
        }

        /// <summary>
        /// Reset internal state.
        /// </summary>
        public void Reset()
        {
            offset = 0;
            bufferFull = false;
        }
    }
}