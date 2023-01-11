using System;


namespace Clatter.Core
{
    /// <summary>
    /// Generates scrape audio.
    ///
    /// A Scrape is a series of continuous events. By repeatedly calling GetAudio(), the scrape event will continue.
    ///
    /// Scrape events are automatically generated from collision data within `AudioGenerator`. You can also manually create a Scrape and use it to generate audio without needing to use an `AudioGenerator`. This can be useful if you want to generate audio without needing to create a physics simulation.
    ///
    /// ## Code Examples
    ///
    /// {code_example:ScrapeAudioExample}
    ///
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
        public static double maxSpeed = 5;
        /// <summary>
        /// The ID of this scrape event. This is used to track an ongoing scrape.
        /// </summary>
        public readonly int scrapeId;
        /// <summary>
        /// The previous index in the scrape surface array.
        /// </summary>
        private int scrapeIndex;
        /// <summary>
        /// A cached buffer for the horizontal force.
        /// </summary>
        private readonly double[] horizontalForce = new double[SAMPLES_LENGTH];
        /// <summary>
        /// A cached buffer for the vertical force.
        /// </summary>
        private readonly double[] verticalForce = new double[SAMPLES_LENGTH];
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
            scrapeId = rng.Next();
        }

        
        /// <summary>
        /// Generate audio. Returns true if audio was generated. This will set the `samples` field.
        /// </summary>
        /// <param name="speed">The collision speed.</param>
        public override bool GetAudio(double speed)
        {
            // Get the speed of the primary object and clamp it.
            double primarySpeed = Math.Min(speed, maxSpeed);
            int numPts = (int)(Math.Floor((primarySpeed / 10) / ScrapeMaterialData.SCRAPE_M_PER_PIXEL) + 1);
            if (numPts <= 1 || numPts >= scrapeMaterialData.d2sdx2.Length)
            {
                return false;
            }
            // Get impulse response of the colliding objects.
            if (!gotImpulseResponse)
            {
                int impulseResponseLength = GetImpulseResponse(AdjustModes(speed), ref impulseResponse);
                if (impulseResponseLength == 0)
                {
                    return false;
                }
                gotImpulseResponse = true;
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
            // Get the horizontal force.
            double curveMass = 10 * primary.mass;
            MedianFilter medianFilter = new MedianFilter(5);
            // Apply interpolation.
            int horizontalInterpolationIndex = 0;
            int verticalInterpolationIndex = 0;
            double vertical = 0.5 * Math.Pow(primarySpeed / maxSpeed, 2);
            double horizontal = 0.05 * (primarySpeed / maxSpeed);
            for (int i = 0; i < SAMPLES_LENGTH; i++)
            {
                // Get the horizontal force.
                horizontalForce[i] = horizontal * ScrapeLinearSpace[i].Interpolate1D(vect1, scrapeMaterialData.dsdx, scrapeMaterialData.dsdx[scrapeIndex], scrapeMaterialData.dsdx[finalIndex], scrapeIndex, ref horizontalInterpolationIndex, numPts);
                // Get the curve value.
                verticalForce[i] = vertical * medianFilter.ProcessSample(Math.Tanh(ScrapeLinearSpace[i].Interpolate1D(vect1, scrapeMaterialData.d2sdx2, scrapeMaterialData.d2sdx2[scrapeIndex], scrapeMaterialData.d2sdx2[finalIndex], scrapeIndex, ref verticalInterpolationIndex, numPts) / curveMass));
            }
            for (int i = 0; i < SAMPLES_LENGTH; i++)
            {
                verticalForce[i] += horizontalForce[i];
            }
            // Convolve and apply roughness.
            impulseResponse.Convolve(verticalForce, ScrapeLinearSpace.Length, ref samples.samples);
            for (int i = 0; i < SAMPLES_LENGTH; i++)
            {
                samples.samples[i] *= scrapeMaterialData.roughnessRatio;
            }
            samples.length = SAMPLES_LENGTH;
            scrapeIndex = finalIndex;
            return true;
        }


        /// <summary>
        /// Returns the number of scrape events given a duration.
        /// </summary>
        /// <param name="duration">The duration of the scrape in seconds.</param>
        public static int GetNumScrapeEvents(double duration)
        {
            return (int)(duration * Globals.framerate / SAMPLES_LENGTH);
        }
        

        /// <summary>
        /// Returns the default size of the samples.samples array.
        /// </summary>
        protected override int GetSamplesSize()
        {
            return SAMPLES_LENGTH;
        }
    }
}