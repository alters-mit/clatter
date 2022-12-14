namespace Clatter.Core
{
    /// <summary>
    /// Misc. utility class.
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// Equivalent to np.linspace. 
        /// This is an optimized version of this: https://github.com/accord-net/framework/blob/development/Sources/Accord.Math/Vector/Vector.Interval.Generated.cs
        /// </summary>
        /// <param name="a">The start value.</param>
        /// <param name="b">The end value. It's assumed that b is greater than a.</param>
        /// <param name="steps">The number of steps.</param>
        public static double[] LinSpace(double a, double b, int steps)
        {
            double[] r = new double[steps];
            double stepSize = (b - a) / (steps - 1);
            for (uint i = 0; i < r.Length - 1; i++)
            {
                r[i] = a + i * stepSize;
            }
            r[r.Length - 1] = b;
            return r;
        }
    }
}