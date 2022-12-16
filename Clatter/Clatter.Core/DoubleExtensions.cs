using System;


namespace Clatter.Core
{
    /// <summary>
    /// Extensions for doubles.
    /// </summary>
    public static class DoubleExtensions
    {
        /// <summary>
        /// Convolve an array with the given kernel.
        /// Source: https://github.com/accord-net/framework/blob/development/Sources/Accord.Math/Matrix/Matrix.Common.cs
        /// This code is a more optimized version of the source.
        /// </summary>
        /// <param name="a">A floating number array.</param>
        /// <param name="kernel">A convolution kernel.</param>
        /// <param name="trim">If true, trim the length of the convolved array to a.Length.</param>
        public static double[] Convolve(this double[] a, double[] kernel, bool trim)
        {
            return Convolve(a, kernel, trim ? a.Length : a.Length + (int)Math.Ceiling(kernel.Length / 2.0));
        }


        /// <summary>
        /// Convolve an array with the given kernel.
        /// Source: https://github.com/accord-net/framework/blob/development/Sources/Accord.Math/Matrix/Matrix.Common.cs
        /// This code is a more optimized version of the source.
        /// </summary>
        /// <param name="a">A floating number array.</param>
        /// <param name="kernel">A convolution kernel.</param>
        /// <param name="length">The length of the convolved array.</param>
        public static double[] Convolve(this double[] a, double[] kernel, int length)
        {
            double[] result = new double[length];
            int k;
            for (int i = 0; i < result.Length; i++)
            {
                for (int j = 0; j < kernel.Length; j++)
                {
                    k = i - j;
                    if (k < 0)
                    {
                        break;
                    }
                    if (k < a.Length)
                    {
                        result[i] += a[k] * kernel[j];
                    }
                }
            }
            return result;
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
        public static double Interpolate1D(this double value, double[] x, double[] y, double lower, double upper, int yIndexOffset, ref int startX)
        {
            int start;
            int next;
            double m;
            for (int i = startX; i < x.Length; i++)
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
            // Convert doubles to int16 and copy the byte data into the array.
            // Source: https://gist.github.com/darktable/2317063
            for (int i = 0; i < length; i++)
            {
                Buffer.BlockCopy(BitConverter.GetBytes((short)(a[i] * Globals.FLOAT_TO_SHORT)), 0, bs, i * 2, 2);
            }
            return bs;
        }


        /// <summary>
        /// Clamp a double.
        /// </summary>
        /// <param name="v">(this)</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        public static double Clamp(this double v, double min, double max)
        {
            if (v < min)
            {
                return min;
            }
            else if (v > max)
            {
                return max;
            }
            else
            {
                return v;
            }
        }
        
        
        /// <summary>
        /// Estimates the median value from the unsorted data array.
        /// WARNING: Works inplace and can thus causes the data array to be reordered.
        /// Source: https://github.com/mathnet/mathnet-numerics/blob/master/src/Numerics/Statistics/ArrayStatistics.cs#L413
        /// </summary>
        /// <param name="data">Sample array, no sorting is assumed. Will be reordered.</param>
        public static double MedianInplace(this double[] data)
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
        public static double Minimum(this double[] data)
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