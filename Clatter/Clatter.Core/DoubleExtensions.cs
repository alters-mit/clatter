﻿using System;


namespace Clatter.Core
{
    /// <summary>
    /// Extensions for doubles.
    /// </summary>
    public static class DoubleExtensions
    {
        /// <summary>
        /// Conversion factor for float to short.
        /// </summary>
        private const int FLOAT_TO_SHORT = 32767;
        
        
        /// <summary>
        /// Clamp this value to be between a and b.
        /// </summary>
        /// <param name="d">(this)</param>
        /// <param name="a">The lower bound.</param>
        /// <param name="b">The upper bound (inclusive).</param>
        public static double Clamp(this double d, double a, double b)
        {
            if (d < a)
            {
                return a;
            }
            else if (d > b)
            {
                return b;
            }
            else
            {
                return d;             
            }
        }


        /// <summary>
        /// Convolve an array with the given kernel.
        /// Source: https://stackoverflow.com/a/7239016
        /// This code is a more optimized version of the source.
        /// </summary>
        /// <param name="a">(this)</param>
        /// <param name="kernel">A convolution kernel.</param>
        /// <param name="length">The length of the convolved array.</param>
        /// <param name="result">The output array.</param>
        public static void Convolve(this double[] a, double[] kernel, int length, ref double[] result)
        {
            if (result.Length < length)
            {
                Array.Resize(ref result, length * 2);
            }
            double sum;
            int n1;
            int n2;
            int inputLength = a.Length;
            int kernelLength = kernel.Length;
            for (int i = length - 1; i >= 0; i--)
            {
                sum = 0;
                n1 = i < inputLength ? 0 : i - inputLength - 1;
                n2 = i < kernelLength ? i : kernelLength - 1;
                for (int j = n1; j <= n2; j++)
                {
                    sum += a[i - j] * kernel[j];
                }
                result[i] = sum;
            }
        }


        /// <summary>
        /// Interpolates data using a piece-wise linear function.
        /// This has been optimized from the source.
        /// Source: https://github.com/accord-net/framework/blob/master/Sources/Accord.Math/Tools.cs#L669
        /// </summary>
        /// <param name="value">The value to be calculated.</param>
        /// <param name="x">The input data points <c>x</c>. Those values need to be sorted.</param>
        /// <param name="y">The output data points <c>y</c>.</param>
        /// <param name="lower">The value to be returned for values before the first point in <paramref name="x"/>.</param>
        /// <param name="upper">The value to be returned for values after the last point in <paramref name="x"/>.</param>
        /// <param name="yIndexOffset">Offset the y index by this value.</param>
        /// <param name="startX">Start interpolating the x array at this index.</param>
        /// <param name="endX">The final index in the x array.</param>
        public static double Interpolate1D(this double value, double[] x, double[] y, double lower, double upper, int yIndexOffset, ref int startX, int endX)
        {
            int start;
            int next;
            double m;
            for (int i = startX; i < endX; i++)
            {
                if (value < x[i])
                {
                    startX = i + 1;
                    if (i == 0)
                    {
                        return lower;
                    }
                    start = i - 1;
                    next = i;
                    m = (value - x[start]) / (x[next] - x[start]);
                    return y[start + yIndexOffset] + (y[next + yIndexOffset] - y[start + yIndexOffset]) * m;
                }
            }
            startX = 0;
            return upper;
        }


        /// <summary>
        /// Returns this array converted to floats.
        /// </summary>
        /// <param name="a">(this)</param>
        /// <param name="length">The length of the converted array.</param>
        public static float[] ToFloats(this double[] a, int length)
        {
            float[] fs = new float[length];
            for (int i = 0; i < length; i++)
            {
                fs[i] = (float)a[i];
            }
            return fs;
        }
        
        
        /// <summary>
        /// Returns this array converted to a byte array of int16s.
        /// </summary>
        /// <param name="a">(this)</param>
        /// <param name="length">The length of the converted array.</param>
        public static byte[] ToInt16Bytes(this double[] a, int length)
        {
            byte[] bs = new byte[length * 2];
            byte[] shortArray = new byte[2];
            short s;
            // Convert doubles to int16 and copy the byte data into the array.
            // Source: https://gist.github.com/darktable/2317063
            for (int i = 0; i < length; i++)
            {
                // Cast to short.
                s = (short)(a[i] * FLOAT_TO_SHORT);
                // Convert to bytes.
                s.GetBytes(shortArray);
                // Copy the bytes
                Buffer.BlockCopy(shortArray, 0, bs, i * 2, 2);
            }
            return bs;
        }

        
        /// <summary>
        /// Estimates the median value from the unsorted data array.
        /// WARNING: Works inplace and can thus causes the data array to be reordered.
        /// Source: https://github.com/mathnet/mathnet-numerics/blob/master/src/Numerics/Statistics/ArrayStatistics.cs#L413
        /// </summary>
        /// <param name="data">Sample array, no sorting is assumed. Will be reordered.</param>
        public static double MedianInPlace(this double[] data)
        {
            int k = data.Length / 2;
            return data.Length % 2 != 0
                ? data.SelectInPlace(k)
                : (data.SelectInPlace(k - 1) + data.SelectInPlace(k)) / 2.0;
        }
        
        
        /// <summary>
        /// Source: https://github.com/mathnet/mathnet-numerics/blob/70d45612af89d3b70661a566c9b82a8982a23f1d/src/Numerics/Statistics/ArrayStatistics.cs#L663
        /// </summary>
        /// <param name="workingData">The data.</param>
        /// <param name="rank">The rank value.</param>
        private static double SelectInPlace(this double[] workingData, int rank)
        {
            // Numerical Recipes: select
            // http://en.wikipedia.org/wiki/Selection_algorithm
            if (rank <= 0)
            {
                return workingData.Minimum();
            }
            if (rank >= workingData.Length - 1)
            {
                return workingData.Maximum();
            }
            double[] a = workingData;
            int low = 0;
            int high = a.Length - 1;
            while (true)
            {
                if (high <= low + 1)
                {
                    if (high == low + 1 && a[high] < a[low])
                    {
                        (a[low], a[high]) = (a[high], a[low]);
                    }

                    return a[rank];
                }
                int middle = (low + high) >> 1;
                (a[middle], a[low + 1]) = (a[low + 1], a[middle]);
                if (a[low] > a[high])
                {
                    (a[low], a[high]) = (a[high], a[low]);
                }
                if (a[low + 1] > a[high])
                {
                    (a[low + 1], a[high]) = (a[high], a[low + 1]);
                }
                if (a[low] > a[low + 1])
                {
                    (a[low], a[low + 1]) = (a[low + 1], a[low]);
                }
                int begin = low + 1;
                int end = high;
                double pivot = a[begin];
                while (true)
                {
                    do
                    {
                        begin++;
                    }
                    while (a[begin] < pivot);

                    do
                    {
                        end--;
                    }
                    while (a[end] > pivot);
                    if (end < begin)
                    {
                        break;
                    }
                    (a[begin], a[end]) = (a[end], a[begin]);
                }
                a[low + 1] = a[end];
                a[end] = pivot;
                if (end >= rank)
                {
                    high = end - 1;
                }
                if (end <= rank)
                {
                    low = begin;
                }
            }
        }
        
        
        /// <summary>
        /// Returns the smallest value from the unsorted data array.
        /// This assumes that the data has a length greater than zero and no NaN values.
        /// Source: https://github.com/mathnet/mathnet-numerics/blob/70d45612af89d3b70661a566c9b82a8982a23f1d/src/Numerics/Statistics/ArrayStatistics.cs#L51
        /// </summary>
        /// <param name="data">Sample array, no sorting is assumed.</param>
        private static double Minimum(this double[] data)
        {
            double min = double.PositiveInfinity;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] < min)
                {
                    min = data[i];
                }
            }
            return min;
        }
        

        /// <summary>
        /// Returns the largest value from the unsorted data array.
        /// This assumes that the data has a length greater than zero and no NaN values.
        /// Source: https://github.com/mathnet/mathnet-numerics/blob/70d45612af89d3b70661a566c9b82a8982a23f1d/src/Numerics/Statistics/ArrayStatistics.cs#L75
        /// </summary>
        /// <param name="data">Sample array, no sorting is assumed.</param>
        private static double Maximum(this double[] data)
        {
            double max = double.NegativeInfinity;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] > max)
                {
                    max = data[i];
                }
            }
            return max;
        }
    }
}