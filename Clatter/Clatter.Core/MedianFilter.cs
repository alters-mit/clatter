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
        readonly double[] _buffer;
        private Dictionary<int, double[]> offsetBuffers = new Dictionary<int, double[]>();
        int _offset;
        bool _bufferFull;

        /// <summary>
        /// Create a Median Filter.
        /// </summary>
        public MedianFilter(int windowSize)
        {
            _buffer = new double[windowSize];
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
            _buffer[_offset = (_offset == 0) ? _buffer.Length - 1 : _offset - 1] = sample;
            _bufferFull |= _offset == 0;
            if (_bufferFull)
            {
                return _buffer.MedianInplace();
            }
            else
            {
                int length = _buffer.Length - _offset;
                // Copy to the offset buffer.
                Buffer.BlockCopy(_buffer, _offset * 8, offsetBuffers[length], 0, offsetBuffers[length].Length * 8);
                return offsetBuffers[length].MedianInplace();
            }
        }

        /// <summary>
        /// Reset internal state.
        /// </summary>
        public void Reset()
        {
            _offset = 0;
            _bufferFull = false;
        }
    }
}