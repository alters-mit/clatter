using System;
using Clatter.Core;
using UnityEngine;


namespace Clatter.Unity
{
    /// <summary>
    /// A sound generated by an scrape event and `ClatterManager`. A ScrapeSound will persist after its audio clip ends, and will try to play more scrape audio; it will be fed new audio from `ClatterManager`. A ScrapeSound will self-destruct when a scrape event actually ends, as announced by `ClatterManager`.
    /// </summary>
    public class ScrapeSound : Sound
    {
        /// <summary>
        /// Scrape audio data.
        /// </summary>
        private class ScrapeAudioData
        {
            /// <summary>
            /// The audio samples.
            /// </summary>
            public float[] data;
            /// <summary>
            /// If true, this object has been allocated and is ready to be playing.
            /// </summary>
            public bool allocated;
        }


        /// <summary>
        /// The number of cached audio samples.
        /// </summary>
        private const int NUM_NEXT_SAMPLES = 3;


        /// <summary>
        /// A ScrapeSound will pre-allocate generated audio for immediate playback. If this field is true, it will also use that audio to fill in gaps of silence that occur while scrape audio is still being generated. The result isn't entirely "realistic" but sounds much better because it eliminates a lot of choppiness.
        /// </summary>
        public static bool fillSilence = true;
        /// <summary>
        /// A cached array of pending audio data.
        /// </summary>
        private readonly ScrapeAudioData[] nextData = new ScrapeAudioData[NUM_NEXT_SAMPLES];
        /// <summary>
        /// The index of the next audio samples.
        /// </summary>
        private int nextDataIndex = -1;
        /// <summary>
        /// A cached start index of a range of zeros in the audio data.
        /// </summary>
        private int z0;
        /// <summary>
        /// A cached boolean flag indicating whether we've found a range of zeros in the audio data.
        /// </summary>
        private bool zeroing;
        /// <summary>
        /// A cached length of a range of zeros in the audio data.
        /// </summary>
        private int zerosLength;
        /// <summary>
        /// A cached index in the audio data when we're allocating scrape audio to each channel.
        /// </summary>
        private int dataIndex;
        /// <summary>
        /// The last valid audio chunk.
        /// </summary>
        private float[] lastValidAudioChunk = new float[0];

        private bool gotLastValidAudioChunk;


        /// <summary>
        /// Update the scrape audio.
        /// </summary>
        /// <param name="samples">The new samples.</param>
        /// <param name="position">The new position of the sound.</param>
        public void UpdateAudio(Samples samples, Vector3d position)
        {
            // Move the scrape sound.
            transform.position = position.ToVector3();
            // Find the next pre-allocated audio chunk and play it.
            if (nextDataIndex < 0)
            {
                for (int i = 0; i < nextData.Length; i++)
                {
                    // Create and use new samples.
                    if (nextData[i] == null)
                    {
                        nextData[i] = new ScrapeAudioData()
                        {
                            data = samples.ToFloats(),
                            allocated = true
                        };
                        nextDataIndex = i;
                        return;
                    }
                    // Use unallocated samples.
                    else if (!nextData[i].allocated)
                    {
                        nextData[i].data = samples.ToFloats();
                        nextData[i].allocated = true;
                        nextDataIndex = i;
                        return;
                    }
                }
            }
        }


        /// <summary>
        /// Invoked whenever an audio clip ends.
        /// </summary>
        protected override void OnAudioClipEnd()
        {
            // Get more samples.
            if (nextDataIndex >= 0 && nextData[nextDataIndex].allocated)
            {
                // Set the audio data.
                Play(nextData[nextDataIndex].data);
                // Un-allocate the samples.
                nextData[nextDataIndex].allocated = false;
                nextDataIndex = -1;
            }
        }


        /// <summary>
        /// This is called automatically in Unity.
        ///
        /// Check for ranges of "zeros" i.e. moments of silence during the scrape and fill it with audio.
        /// </summary>
        /// <param name="data">The chunk of audio data.</param>
        /// <param name="channels">The number of channels.</param>
        private void OnAudioFilterRead(float[] data, int channels)
        {
            // We're not trying to fill silence.
            if (!fillSilence)
            {
                return;
            }
            // Resize the last valid audio chunk.
            if (lastValidAudioChunk.Length != data.Length)
            {
                lastValidAudioChunk = new float[data.Length];
            }
            // Start by resenting the cached index variables.
            z0 = 0;
            zeroing = false;
            // Iterate through the data, incrementing by the number of channels.
            for (int i = 0; i < data.Length; i += channels)
            {
                if (data[i] == 0)
                {
                    // This is the start of a chunk of zeros.
                    if (!zeroing)
                    {
                        z0 = i;
                        zeroing = true; 
                    }
                }
                else
                {
                    // We found a range of zeros. Now we need to fill it with audio.
                    if (zeroing)
                    {
                        FillZeros(i, data);
                    }
                }
            }
            // We didn't find an end to the range of zeros. Fill everything starting at z0.
            if (zeroing)
            {
                FillZeros(data.Length, data);
            }
            // Remember the last valid audio chunk.
            else
            {
                Buffer.BlockCopy(data, 0, lastValidAudioChunk, 0, data.Length * 4);
                gotLastValidAudioChunk = true;
            }
        }
        
        
        /// <summary>
        /// Fill a portion the audio data array that contains only zeros to ensure that the scrape sound is always continuous.
        /// </summary>
        /// <param name="z1">The end index of the range of zeros.</param>
        /// <param name="data">The chunk of audio data.</param>
        private void FillZeros(int z1, float[] data)
        {
            zeroing = false;
            // Try to use the last valid audio chunk because this is a much faster operation.
            if (gotLastValidAudioChunk)
            {
                Buffer.BlockCopy(lastValidAudioChunk, 0, data, z0 * 4, (z1 - z0) * 4);
            }
        }
    }
}