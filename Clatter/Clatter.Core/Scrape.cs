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
        /// The default impulse response length.
        /// </summary>
        private const int DEFAULT_IMPULSE_RESPONSE_LENGTH = 9000;
        
        
        /// <summary>
        /// When setting the amplitude for a scrape, multiply `AudioEvent.simulationAmp` by this factor.
        /// </summary>
        public static double scrapeAmp = 1;
        /// <summary>
        /// For the purposes of scrape audio generation, the collision speed is clamped to this maximum value in meters per second.
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
        /// A cached buffer for the force.
        /// </summary>
        private readonly double[] force = new double[SAMPLES_LENGTH];
        /// <summary>
        /// The scrape material data for this scrape.
        /// </summary>
        private readonly ScrapeMaterialData scrapeMaterialData;
        /// <summary>
        /// The cached impulse response array.
        /// </summary>
        private double[] impulseResponse = new double[DEFAULT_IMPULSE_RESPONSE_LENGTH];
        /// <summary>
        /// If true, we've generated the impulse response.
        /// </summary>
        private bool gotImpulseResponse;
        /// <summary>
        /// The cached linear space array. The length of this can change depending on the speed of the scrape.
        /// </summary>
        private double[] linearSpace = new double[DEFAULT_IMPULSE_RESPONSE_LENGTH];
        /// <summary>
        /// A cached median filter used for smoothing over the sound.
        /// </summary>
        private readonly MedianFilter medianFilter = new MedianFilter(5);
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
        public Scrape(ScrapeMaterial scrapeMaterial, ClatterObjectData primary, ClatterObjectData secondary, Random rng) : base(primary, secondary, rng)
        {
            scrapeMaterialData = ScrapeMaterialData.Get(scrapeMaterial);
            scrapeId = rng.Next();
        }

        
        /// <summary>
        /// Generate audio. Returns true if audio was generated. This will set the `samples` field.
        /// </summary>
        /// <param name="speed">The collision speed in meters per second.</param>
        /// <param name="pregeneratedImpulseResponse">An optional pre-generated impulse response array. If null, impulse response will be generated at runtime.</param>
        public override bool GetAudio(double speed, double[] pregeneratedImpulseResponse = null)
        {
            double scrapeSpeed = Math.Min(speed, maxSpeed);
            int numPts = (int)(Math.Floor((scrapeSpeed / 10) / ScrapeMaterialData.SCRAPE_M_PER_PIXEL) + 1);
            if (numPts <= 1 || numPts >= scrapeMaterialData.d2sdx2.Length)
            {
                return false;
            }
            // Get impulse response of the colliding objects.
            if (!gotImpulseResponse)
            {
                int impulseResponseLength;
                // Generate an impulse response.
                if (pregeneratedImpulseResponse == null)
                {
                    impulseResponseLength = GetImpulseResponse(AdjustModes(speed), ref this.impulseResponse);
                }
                // Use the prerecorded impulse response.
                else
                {
                    Buffer.BlockCopy(pregeneratedImpulseResponse, 0, impulseResponse, 0, pregeneratedImpulseResponse.Length * 8);
                    impulseResponseLength = pregeneratedImpulseResponse.Length;
                }
                if (impulseResponseLength == 0)
                {
                    return false;
                }
                gotImpulseResponse = true;
            }
            // Get the final index.
            int finalIndex = scrapeIndex + numPts;
            // Define a linear space.
            LinSpace.GetInPlace(0.0, 1.0, numPts, ref linearSpace);
            // Reset the indices if they exceed the scrape surface.
            if (finalIndex >= scrapeMaterialData.dsdx.Length)
            {
                scrapeIndex = 0;
                finalIndex = numPts;
            }
            // Calculate the force by adding the horizontal force and the vertical force.
            // The horizontal force is the interpolation of the dsdx array multiplied by a factor.
            // The vertical force is a median filter sample of tanh of (the interpolation of the d2sdx2 array multiplied by a factor).
            int horizontalInterpolationIndex = 0;
            int verticalInterpolationIndex = 0;
            double vertical = 0.5 * Math.Pow(scrapeSpeed / maxSpeed, 2);
            double horizontal = 0.05 * (scrapeSpeed / maxSpeed);
            double curveMass = 10 * primary.mass;
            for (int i = 0; i < SAMPLES_LENGTH; i++)
            {
                force[i] = (horizontal * ScrapeLinearSpace[i].Interpolate1D(linearSpace, scrapeMaterialData.dsdx, 
                    scrapeMaterialData.dsdx[scrapeIndex], scrapeMaterialData.dsdx[finalIndex], scrapeIndex, 
                    ref horizontalInterpolationIndex, numPts)) + 
                           (vertical * medianFilter.ProcessSample(Math.Tanh(ScrapeLinearSpace[i].Interpolate1D(linearSpace, 
                               scrapeMaterialData.d2sdx2, scrapeMaterialData.d2sdx2[scrapeIndex],
                               scrapeMaterialData.d2sdx2[finalIndex], scrapeIndex, 
                               ref verticalInterpolationIndex, numPts) / curveMass)));
            }
            // Convolve.
            this.impulseResponse.Convolve(force, SAMPLES_LENGTH, ref samples.samples);
            // Apply roughness and amp.
            double a = scrapeMaterialData.roughnessRatio * simulationAmp * scrapeAmp;
            for (int i = 0; i < SAMPLES_LENGTH; i++)
            {
                samples.samples[i] *= a;
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