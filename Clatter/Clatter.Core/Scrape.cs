using System;


namespace Clatter.Core
{
    /// <summary>
    /// Audio data for a scrape event.
    /// </summary>
    public class Scrape : AudioEvent
    {
        /// <summary>
        /// The length of the scrape samples.
        /// </summary>
        public const int SAMPLES_LENGTH = 4410;
        /// <summary>
        /// Conversion factor from short to double. This is relevant for the decibel values.
        /// </summary>
        private const double SHORT_CONVERSION = 1.0 / 32767;


        /// <summary>
        /// The maximum speed allowed for a scrape.
        /// </summary>
        public static double scrapeMaxSpeed = 1;
        /// <summary>
        /// The audio source ID. We need to declare this here because scrapes are ongoing sounds.
        /// </summary>
        public readonly int audioSourceID;
        /// <summary>
        /// The previous index in the scrape surface array.
        /// </summary>
        private int scrapeIndex;
        /// <summary>
        /// A cached buffer for the horizontal force.
        /// </summary>
        private readonly double[] horizontalForce = new double[ScrapeLinearSpace.Length];
        /// <summary>
        /// A cached buffer for the vertical force.
        /// </summary>
        private readonly double[] verticalForce = new double[ScrapeLinearSpace.Length];
        /// <summary>
        /// A cached array of the summed master.
        /// </summary>
        private readonly double[] summedMaster = new double[Globals.framerateInt * 10];
        /// <summary>
        /// The scrape material data for this scrape.
        /// </summary>
        private readonly ScrapeMaterialData scrapeMaterialData;
        /// <summary>
        /// A linear space vector used for scrape synthesis.
        /// </summary>
        private static readonly double[] ScrapeLinearSpace = Util.LinSpace(0.0, 1.0, SAMPLES_LENGTH);


        public Scrape(ScrapeMaterial scrapeMaterial, AudioObjectData primary, AudioObjectData secondary, Random rng) : base(primary, secondary, rng)
        {
            scrapeMaterialData = ScrapeMaterialData.Get(scrapeMaterial);
            audioSourceID = rng.Next();
        }


        public override bool GetAudio(CollisionEvent collisionEvent, Random rng)
        {
            // Get the speed of the primary object and clamp it.
            double speed = Math.Min(collisionEvent.primary.speed, scrapeMaxSpeed);
            double db2 = (40 * Math.Log10(speed / scrapeMaxSpeed) - 4) * SHORT_CONVERSION;
            double db1 = (20 * Math.Log10(speed / scrapeMaxSpeed) - 25) * SHORT_CONVERSION;
            // Get impulse response of the colliding objects.
            double[] impulseResponse;
            if (!GetImpact(collisionEvent, rng, out impulseResponse))
            {
                return false;
            }
            int numPts = (int)(Math.Floor((speed / 10) / ScrapeMaterialData.SCRAPE_M_PER_PIXEL) + 1);
            if (numPts <= 1)
            {
                return false;
            }
            // Get the final index.
            int finalIndex = scrapeIndex + numPts;
            // Define a linear space.
            double[] vect1 = Util.LinSpace(0.0, 1.0, numPts);
            // Reset the indices if they exceed the scrape surface.
            if (finalIndex >= scrapeMaterialData.dsdx.Length)
            {
                scrapeIndex = 0;
                finalIndex = numPts;
            }
            int length = finalIndex - scrapeIndex;
            // Get the horizontal force.
            double curveMass = 1000 * collisionEvent.primary.mass;
            MedianFilter medianFilter = new MedianFilter(5);
            // Apply interpolation.
            int horizontalInterpolationIndex = 0;
            int verticalInterpolationIndex = 0;
            for (int i = 0; i < ScrapeLinearSpace.Length; i++)
            {
                // Get the horizontal force.
                horizontalForce[i] = ScrapeLinearSpace[i].Interpolate1D(vect1, scrapeMaterialData.dsdx, scrapeMaterialData.dsdx[scrapeIndex], scrapeMaterialData.dsdx[scrapeIndex + length], scrapeIndex, ref horizontalInterpolationIndex);
                // Get the curve value.
                verticalForce[i] = medianFilter.ProcessSample(Math.Tanh(ScrapeLinearSpace[i].Interpolate1D(vect1, scrapeMaterialData.d2sdx2, scrapeMaterialData.d2sdx2[scrapeIndex], scrapeMaterialData.d2sdx2[scrapeIndex + length], scrapeIndex, ref verticalInterpolationIndex)
                    / curveMass));
            }
            double maxAbsVerticalForce = 0;
            double q;
            for (int i = 0; i < verticalForce.Length; i++)
            {
                q = Math.Abs(verticalForce[i]);
                if (q > maxAbsVerticalForce)
                {
                    maxAbsVerticalForce = q;
                }
            }
            for (int i = 0; i < verticalForce.Length; i++)
            {
                verticalForce[i] /= maxAbsVerticalForce;
            }
            // Convolve and apply roughness.
            double[] conv1 = impulseResponse.Convolve(verticalForce, ScrapeLinearSpace.Length);
            double[] conv2 = impulseResponse.Convolve(horizontalForce, ScrapeLinearSpace.Length);
            int c = 0;
            for (int i = 0; i < conv1.Length; i++)
            {
                summedMaster[i] = ((conv1[c] * db1) + (conv2[c] * db2)) * scrapeMaterialData.roughnessGain;
                c++;
            }
            // Generate the samples.
            samples.Set(summedMaster, start: 0, length: ScrapeLinearSpace.Length);
            scrapeIndex = finalIndex;
            return true;
        }
    }
}