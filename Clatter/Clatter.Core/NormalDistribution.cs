using System;


namespace Clatter.Core
{
    /// <summary>
    /// This class has been copied from Accord.net because we only need one method from the entire package.
    /// Source: https://github.com/accord-net/framework/blob/development/Sources/Accord.Statistics/Distributions/Univariate/Continuous/NormalDistribution.cs
    /// </summary>
    public static class NormalDistribution
    {
        [ThreadStatic]
        private static bool useSecond;
        [ThreadStatic]
        private static double secondValue;


        /// <summary>
        /// Random Gaussian distribution.
        /// </summary>
        /// <param name="mean">The mean.</param>
        /// <param name="stdDev">The standard deviation.</param>
        /// <param name="source">The random source.</param>
        public static double Random(double mean, double stdDev, Random source)
        {
            double v;
            // check if we can use second value
            if (useSecond)
            {
                // return the second number
                useSecond = false;
                v = secondValue;
            }
            else
            {
                double x1, x2, w, firstValue;
                // generate new numbers
                do
                {
                    x1 = source.NextDouble() * 2.0 - 1.0;
                    x2 = source.NextDouble() * 2.0 - 1.0;
                    w = x1 * x1 + x2 * x2;
                }
                while (w >= 1.0);
                w = Math.Sqrt((-2.0 * Math.Log(w)) / w);
                // get two standard random numbers
                firstValue = x1 * w;
                secondValue = x2 * w;
                useSecond = true;
                v = firstValue;
            }
            return v * stdDev + mean;
        }
    }
}