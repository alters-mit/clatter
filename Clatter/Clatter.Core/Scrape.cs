using System;
using ClatterRs;


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
        private readonly MedianFilter medianFilter = new MedianFilter();
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
        /// <param name="impulseResponsePath">Optional. If included, this is the path to a file containing impulse response data.</param>
        public override bool GetAudio(double speed, string impulseResponsePath = null)
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
                double amp = AdjustModes(speed);
                int impulseResponseLength = impulseResponsePath == null ? GetImpulseResponse(amp, ref impulseResponse) : LoadImpulseResponse(impulseResponsePath, amp, ref impulseResponse);
                if (impulseResponseLength == 0)
                {
                    return false;
                }
                gotImpulseResponse = true;
            }
            if (Globals.CanUseNativeLibrary)
            {
                unsafe
                {
                    fixed (double* dsdxPointer = scrapeMaterialData.dsdx, 
                           d2sdx2Pointer = scrapeMaterialData.d2sdx2, 
                           impulseResponsePointer = impulseResponse, 
                           forcePointer = force, 
                           linearSpacePointer = linearSpace, 
                           samplesPointer = samples.samples, 
                           medianBuffer5Pointer = medianFilter.buffer, 
                           medianBuffer1Pointer = medianFilter.offsetBuffer1,
                           medianBuffer2Pointer = medianFilter.offsetBuffer2,
                           medianBuffer3Pointer = medianFilter.offsetBuffer3,
                           medianBuffer4Pointer = medianFilter.offsetBuffer4)
                    {
                        UIntPtr dsdxLength = (UIntPtr)scrapeMaterialData.dsdx.Length;
                        Vec_double_t dsdx = new Vec_double_t
                        {
                            ptr = dsdxPointer,
                            len = dsdxLength,
                            cap = dsdxLength
                        };
                        UIntPtr d2sdx2Length = (UIntPtr)scrapeMaterialData.d2sdx2.Length;
                        Vec_double_t d2sdx2 = new Vec_double_t
                        {
                            ptr = d2sdx2Pointer,
                            len = d2sdx2Length,
                            cap = d2sdx2Length
                        };
                        ScrapeMaterialData_t nativeScrapeMaterialData = new ScrapeMaterialData_t
                        {
                            dsdx = dsdx,
                            d2sdx2 = d2sdx2,
                            roughness_ratio = scrapeMaterialData.roughnessRatio
                        };
                        UIntPtr impulseResponseLength = (UIntPtr)impulseResponse.Length;
                        Vec_double_t impulseResponseVec = new Vec_double_t
                        {
                            ptr = impulseResponsePointer,
                            len = impulseResponseLength,
                            cap = impulseResponseLength
                        };
                        UIntPtr forceLength = (UIntPtr)force.Length;
                        Vec_double_t forceVec = new Vec_double_t
                        {
                            ptr = forcePointer,
                            len = forceLength,
                            cap = forceLength
                        };
                        UIntPtr linearSpaceLength = (UIntPtr)linearSpace.Length;
                        Vec_double_t linearSpaceVec = new Vec_double_t
                        {
                            ptr = linearSpacePointer,
                            len = linearSpaceLength,
                            cap = linearSpaceLength
                        };
                        UIntPtr samplesLength = (UIntPtr)samples.samples.Length;
                        Vec_double_t samplesVec = new Vec_double_t
                        {
                            ptr = samplesPointer,
                            len = samplesLength,
                            cap = samplesLength
                        };
                        UIntPtr medianBuffer5Length = (UIntPtr)medianFilter.buffer.Length;
                        Vec_double_t medianBuffer5 = new Vec_double_t
                        {
                            ptr = medianBuffer5Pointer,
                            len = medianBuffer5Length,
                            cap = medianBuffer5Length
                        };
                        UIntPtr medianBuffer1Length = (UIntPtr)medianFilter.offsetBuffer1.Length;
                        Vec_double_t medianBuffer1 = new Vec_double_t
                        {
                            ptr = medianBuffer1Pointer,
                            len = medianBuffer1Length,
                            cap = medianBuffer1Length
                        };
                        UIntPtr medianBuffer2Length = (UIntPtr)medianFilter.offsetBuffer2.Length;
                        Vec_double_t medianBuffer2 = new Vec_double_t
                        {
                            ptr = medianBuffer2Pointer,
                            len = medianBuffer2Length,
                            cap = medianBuffer2Length
                        };
                        UIntPtr medianBuffer3Length = (UIntPtr)medianFilter.offsetBuffer3.Length;
                        Vec_double_t medianBuffer3 = new Vec_double_t
                        {
                            ptr = medianBuffer3Pointer,
                            len = medianBuffer3Length,
                            cap = medianBuffer3Length
                        };
                        UIntPtr medianBuffer4Length = (UIntPtr)medianFilter.offsetBuffer4.Length;
                        Vec_double_t medianBuffer4 = new Vec_double_t
                        {
                            ptr = medianBuffer4Pointer,
                            len = medianBuffer4Length,
                            cap = medianBuffer4Length
                        };
                        MedianFilter_t nativeMedianFilter = new MedianFilter_t
                        {
                            buffer = medianBuffer5,
                            offset_buffer_1 = medianBuffer1,
                            offset_buffer_2 = medianBuffer2,
                            offset_buffer_3 = medianBuffer3,
                            offset_buffer_4 = medianBuffer4,
                            offset = (UIntPtr)medianFilter.offset,
                            full = medianFilter.bufferFull
                        };
                        
                        // Generate scrape audio.
                        scrapeIndex = (int)Ffi.get_scrape(primary.mass, scrapeSpeed, maxSpeed, simulationAmp, scrapeAmp,
                            (UIntPtr)numPts, (UIntPtr)scrapeIndex, &nativeScrapeMaterialData, &impulseResponseVec, 
                            &linearSpaceVec, &forceVec, &nativeMedianFilter, &samplesVec);
                        
                        // Update the median filter.
                        medianFilter.offset = (int)nativeMedianFilter.offset;
                        medianFilter.bufferFull = nativeMedianFilter.full;
                    }
                }
            }
            else
            { 
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
                impulseResponse.Convolve(force, SAMPLES_LENGTH, ref samples.samples);
                // Apply roughness and amp.
                double a = scrapeMaterialData.roughnessRatio * simulationAmp * scrapeAmp;
                for (int i = 0; i < SAMPLES_LENGTH; i++)
                {
                    samples.samples[i] *= a;
                }
                scrapeIndex = finalIndex;
            }
            samples.length = SAMPLES_LENGTH;
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