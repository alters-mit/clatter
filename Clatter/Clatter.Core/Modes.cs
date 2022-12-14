using System;


namespace Clatter.Core
{
    /// <summary>
    /// Audio modes data for an object.
    /// </summary>
    public class Modes
    {
        /// <summary>
        /// The default length of the arrays.
        /// </summary>
        private const int DEFAULT_LENGTH = 10;


        /// <summary>
        /// Mode frequencies in Hz.
        /// </summary>
        public readonly double[] frequencies = new double[DEFAULT_LENGTH];
        /// <summary>
        /// Mode onset powers in dB re 1.
        /// </summary>
        public readonly double[] powers = new double[DEFAULT_LENGTH];
        /// <summary>
        /// Mode decay times i.e. the time in ms it takes for each mode to decay 60dB from its onset power.
        /// </summary>
        public readonly double[] decayTimes = new double[DEFAULT_LENGTH];


        /// <summary>
        /// Generate object modes data.
        /// </summary>
        /// <param name="material">The impact material data.</param>
        /// <param name="rng">The random number generator.</param>
        public Modes(ImpactMaterialData material, Random rng)
        {
            for (int jm = 0; jm < 10; jm++)
            {
                double jf = 0;
                while (jf < 20)
                {
                    jf = material.cf[jm] + NormalDistribution.Random(0, material.cf[jm] / 10, rng);
                }
                double jp = material.op[jm] + NormalDistribution.Random(0, 10, rng);
                double jt = 0;
                while (jt < 0.001f)
                {
                    jt = material.rt[jm] + NormalDistribution.Random(0, material.rt[jm] / 10, rng);
                }
                frequencies[jm] = jf;
                powers[jm] = jp;
                decayTimes[jm] = jt * 1e3;
            }
        }


        /// <summary>
        /// Create a mode time-series from mode properties and sum them together.
        /// Returns a synthesized sound.
        /// </summary>
        /// <param name="resonance">The object's audio resonance value.</param>
        public double[] Sum(double resonance)
        {
            double[] synthSound = new double[0];
            for (int i = 0; i < frequencies.Length; i++)
            {
                double H_dB = 80 + powers[i];
                double L_ms = decayTimes[i] * H_dB / 60;
                int modeCount = (int)Math.Ceiling(L_ms / 1e3 * Globals.framerate);
                // Clamp the count to positive values.
                if (modeCount < 0)
                {
                    modeCount = 0;
                }
                double[] mode = new double[modeCount];
                if (modeCount > 0)
                {
                    // Synthesize a sinusoid.
                    double pow = Math.Pow(10, powers[i] / 20);
                    double dcy = -60 / (decayTimes[i] * resonance / 1e3);
                    double q = 2 * frequencies[i] * Math.PI;
                    for (int j = 0; j < modeCount; j++)
                    {
                        double tt = j / Globals.framerate;
                        mode[j] = Math.Cos(tt * q) * pow * Math.Pow(10, (tt * dcy) / 20);
                    }
                }
                if (i == 0)
                {
                    synthSound = mode;
                }
                else
                {
                    synthSound = Add(synthSound, mode);
                }
            }
            return synthSound;
        }


        /// <summary>
        /// Adjust the powers.
        /// </summary>
        /// <param name="rng">The random number generator.</param>
        public void AdjustPowers(Random rng)
        {
            for (int i = 0; i < powers.Length; i++)
            {
                powers[i] += NormalDistribution.Random(0, 2, rng);
            }
        }


        /// <summary>
        /// Add together arrays of different lengths by zero-padding the shorter.
        /// </summary>
        /// <param name="a">The first array.</param>
        /// <param name="b">The second array.</param>
        public static double[] Add(double[] a, double[] b)
        {
            double[] c;
            if (a.Length < b.Length)
            {
                c = new double[b.Length];
                b.CopyTo(c, 0);
                for (int i = 0; i < a.Length; i++)
                {
                    c[i] += a[i];
                }
            }
            else
            {
                c = new double[a.Length];
                a.CopyTo(c, 0);
                for (int i = 0; i < b.Length; i++)
                {
                    c[i] += b[i];
                }
            }
            return c;
        }
    }
}