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
        /// The maximum speed allowed for a scrape.
        /// </summary>
        public static double scrapeMaxSpeed = 5;
        /// <summary>
        /// The audio source ID. We need to declare this here because scrapes are ongoing sounds.
        /// </summary>
        public readonly int audioSourceId;
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
        /// The cached impulse response array. This never gets used.
        /// </summary>
        private double[] impulseResponse = new double[9000];
        /// <summary>
        /// If true, we've generated the impulse response.
        /// </summary>
        private bool gotImpulseResponse;
        /// <summary>
        /// The cached vect1 array.
        /// </summary>
        private double[] vect1 = new double[9000];
        /// <summary>
        /// A linear space vector used for scrape synthesis.
        /// </summary>
        private static readonly double[] ScrapeLinearSpace = LinSpace.Get(0.0, 1.0, SAMPLES_LENGTH);


        /// <summary>
        /// (constructor)
        /// </summary>
        /// <param name="scrapeMaterial">The scrape material.</param>
        /// <param name="primary">The primary object (the smaller, moving object).</param>
        /// <param name="secondary">The secondary object (the scrape surface).</param>
        /// <param name="rng">The random number generator.</param>
        public Scrape(ScrapeMaterial scrapeMaterial, AudioObjectData primary, AudioObjectData secondary, Random rng) : base(primary, secondary, rng)
        {
            scrapeMaterialData = ScrapeMaterialData.Get(scrapeMaterial);
            audioSourceId = rng.Next();
        }


        public override bool GetAudio(CollisionEvent collisionEvent, Random rng)
        {
            // Get the speed of the primary object and clamp it.
            double speed = Math.Min(collisionEvent.primary.speed, scrapeMaxSpeed);
            // Get impulse response of the colliding objects.
            if (!gotImpulseResponse)
            {
                if (!GetImpact(collisionEvent, rng, out impulseResponse))
                {
                    return false;
                }
                gotImpulseResponse = true;
            }
            int numPts = (int)(Math.Floor((speed / 10) / ScrapeMaterialData.SCRAPE_M_PER_PIXEL) + 1);
            if (numPts <= 1)
            {
                return false;
            }
            // Get the final index.
            int finalIndex = scrapeIndex + numPts;
            // Define a linear space.
            LinSpace.GetInPlace(0.0, 1.0, numPts, ref vect1);
            // Reset the indices if they exceed the scrape surface.
            if (finalIndex >= scrapeMaterialData.dsdx.Length)
            {
                scrapeIndex = 0;
                finalIndex = numPts;
            }
            int length = finalIndex - scrapeIndex;
            // Get the horizontal force.
            double curveMass = 10 * collisionEvent.primary.mass;
            MedianFilter medianFilter = new MedianFilter(5);
            // Apply interpolation.
            int horizontalInterpolationIndex = 0;
            int verticalInterpolationIndex = 0;
            double vertical = 0.5 * Math.Pow(speed / scrapeMaxSpeed, 2);
            double horizontal = 0.05 * (speed / scrapeMaxSpeed);
            for (int i = 0; i < ScrapeLinearSpace.Length; i++)
            {
                // Get the horizontal force.
                horizontalForce[i] = horizontal * ScrapeLinearSpace[i].Interpolate1D(vect1, scrapeMaterialData.dsdx, scrapeMaterialData.dsdx[scrapeIndex], scrapeMaterialData.dsdx[scrapeIndex + length], scrapeIndex, ref horizontalInterpolationIndex, numPts);
                // Get the curve value.
                verticalForce[i] = vertical * medianFilter.ProcessSample(Math.Tanh(ScrapeLinearSpace[i].Interpolate1D(vect1, scrapeMaterialData.d2sdx2, scrapeMaterialData.d2sdx2[scrapeIndex], scrapeMaterialData.d2sdx2[scrapeIndex + length], scrapeIndex, ref verticalInterpolationIndex, numPts)
                    / curveMass));
            }
            for (int i = 0; i < verticalForce.Length; i++)
            {
                verticalForce[i] += horizontalForce[i];
            }
            // Convolve and apply roughness.
            double[] conv = impulseResponse.Convolve(verticalForce, ScrapeLinearSpace.Length);
            int c = 0;
            for (int i = 0; i < conv.Length; i++)
            {
                summedMaster[i] = conv[c] * scrapeMaterialData.roughnessRatio;
                c++;
            }
            // Generate the samples.
            samples.Set(summedMaster, start: 0, length: ScrapeLinearSpace.Length);
            scrapeIndex = finalIndex;
            return true;
        }
    }
}