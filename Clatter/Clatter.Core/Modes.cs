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

        public double[] synthSound = new double[9000];
        public int synthSoundLength;
        private double[] mode = new double[9000];


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
        public void Sum(double resonance)
        {
            for (int i = 0; i < frequencies.Length; i++)
            {
                int modeCount = (int)Math.Ceiling((decayTimes[i] * (80 + powers[i]) / 60) / 1e3 * Globals.framerate);
                // Clamp the count to positive values.
                if (modeCount < 0)
                {
                    modeCount = 0;
                }
                // Resize the mode array.
                if (mode.Length < modeCount)
                {
                    Array.Resize(ref mode, modeCount * 2);
                }
                if (modeCount > 0)
                {
                    // Synthesize a sinusoid.
                    double pow = Math.Pow(10, powers[i] / 20);
                    double dcy = -60 / (decayTimes[i] * resonance / 1e3) / 20;
                    double q = 2 * frequencies[i] * Math.PI;
                    double tt;
                    for (int j = 0; j < modeCount; j++)
                    {
                        tt = j / Globals.framerate;
                        mode[j] = Math.Cos(tt * q) * pow * Math.Pow(10, tt * dcy);
                    }
                }
                if (i == 0)
                {
                    // Copy the first mode into the synth sound.
                    synthSoundLength = modeCount;
                    if (synthSound.Length < synthSoundLength)
                    {
                        Array.Resize(ref synthSound, synthSoundLength * 2);
                    }
                    Buffer.BlockCopy(mode, 0, synthSound, 0, synthSoundLength * 8);
                }
                else
                {
                    synthSoundLength = Add(synthSound, synthSoundLength, mode, modeCount, ref synthSound);
                }
            }
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
        /// <param name="aLength">The length of the first array (can be less than the true length).</param>
        /// <param name="b">The second array.</param>
        /// <param name="bLength">The length of the second array (can be less than the true length).</param>
        /// <param name="added">The output array.</param>
        public static int Add(double[] a, int aLength, double[] b, int bLength, ref double[] added)
        {
            if (aLength < bLength)
            {
                if (added.Length < bLength)
                {
                    Array.Resize(ref added, bLength * 2);
                }
                b.CopyTo(added, 0);
                for (int i = 0; i < aLength; i++)
                {
                    added[i] += a[i];
                }
            }
            else
            {
                if (added.Length < aLength)
                {
                    Array.Resize(ref added, aLength * 2);
                }
                a.CopyTo(added, 0);
                for (int i = 0; i < bLength; i++)
                {
                    added[i] += b[i];
                }
            }
            return added.Length;
        }
    }
}