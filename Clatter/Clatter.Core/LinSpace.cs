using System;


namespace Clatter.Core
{
    /// <summary>
    /// Linear space generator.
    /// </summary>
    public static class LinSpace
    {
        /// <summary>
        /// Equivalent to np.linspace. 
        /// This is an optimized version of this: https://github.com/accord-net/framework/blob/development/Sources/Accord.Math/Vector/Vector.Interval.Generated.cs
        /// </summary>
        /// <param name="a">The start value.</param>
        /// <param name="b">The end value. It's assumed that b is greater than a.</param>
        /// <param name="steps">The number of steps.</param>
        public static double[] Get(double a, double b, int steps)
        {
            double[] r = new double[steps];
            GetInPlace(a, b, steps, ref r);
            return r;
        }
        
        
        /// <summary>
        /// Equivalent to np.linspace.
        /// The array is generated in-place to avoid memory allocation.
        /// This is an optimized version of this: https://github.com/accord-net/framework/blob/development/Sources/Accord.Math/Vector/Vector.Interval.Generated.cs
        /// </summary>
        /// <param name="a">The start value.</param>
        /// <param name="b">The end value. It's assumed that b is greater than a.</param>
        /// <param name="steps">The number of steps.</param>
        /// <param name="arr">The array. This will be resized if needed.</param>
        public static void GetInPlace(double a, double b, int steps, ref double[] arr)
        {
            if (arr.Length < steps)
            {
                Array.Resize(ref arr, steps * 2);
            }
            double stepSize = (b - a) / (steps - 1);
            for (uint i = 0; i < arr.Length - 1; i++)
            {
                arr[i] = a + i * stepSize;
            }
            arr[arr.Length - 1] = b;
        }
    }
}