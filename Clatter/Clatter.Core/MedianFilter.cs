using System;
using System.Collections.Generic;


namespace Clatter.Core
{
    /// <summary>
    /// Median-Filters are non-linear filters, returning the median of a sample window as output.
    ///
    /// Median-Filters perform well for de-noise applications where it's important to not loose sharp steps/edges.
    ///
    /// This is an optimized version of this class: https://github.com/mathnet/mathnet-filtering/blob/master/src/Filtering/Median/OnlineMedianFilter.cs
    /// </summary>
    internal class MedianFilter
    {
        /// <summary>
        /// The filter window size.
        /// </summary>
        private const int WINDOW_SIZE = 5;
        
        
        /// <summary>
        /// The filter buffer.
        /// </summary>
        internal readonly double[] buffer;
        internal readonly double[] offsetBuffer1;
        internal readonly double[] offsetBuffer2;
        internal readonly double[] offsetBuffer3;
        internal readonly double[] offsetBuffer4;
        /// <summary>
        /// The current offset.
        /// </summary>
        internal int offset;
        /// <summary>
        /// If true, the buffer is full.
        /// </summary>
        internal bool bufferFull;
        

        /// <summary>
        /// Create a Median Filter.
        /// </summary>
        internal MedianFilter()
        {
            // Set the buffer.
            buffer = new double[WINDOW_SIZE];
            // Generate offset buffers.
            offsetBuffer1 = new double[1];
            offsetBuffer2 = new double[2];
            offsetBuffer3 = new double[3];
            offsetBuffer4 = new double[4];
        }
        
        
        /// <summary>
        /// Process a single sample.
        /// </summary>
        /// <param name="sample">The sample.</param>
        internal double ProcessSample(double sample)
        {
            buffer[offset = (offset == 0) ? buffer.Length - 1 : offset - 1] = sample;
            bufferFull |= offset == 0;
            if (bufferFull)
            {
                return buffer.MedianInPlace();
            }
            else
            {
                int length = buffer.Length - offset;
                double[] offsetBuffer;
                if (length == 1)
                {
                    offsetBuffer = offsetBuffer1;
                }
                else if (length == 2)
                {
                    offsetBuffer = offsetBuffer2;
                }
                else if (length == 3)
                {
                    offsetBuffer = offsetBuffer3;
                }
                else if (length == 4)
                {
                    offsetBuffer = offsetBuffer4;
                }
                else
                {
                    throw new Exception("Invalid offset buffer length: " + length);
                }
                // Copy to the offset buffer.
                Buffer.BlockCopy(buffer, offset * 8, offsetBuffer, 0, offsetBuffer.Length * 8);
                return offsetBuffer.MedianInPlace();
            }
        }
    }
}