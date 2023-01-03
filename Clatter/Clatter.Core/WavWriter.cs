using System;
using System.IO;


namespace Clatter.Core
{
    /// <summary>
    /// Write audio samples to a .wav file. Instantiate a WavWriter to begin writing audio. Call Write(data) to continuously write chunks of audio data to the file. Call End() to stop writing and append the wav header data to the file.
    ///
    /// {code_example:ScrapeAudioExample}
    /// 
    /// </summary>
    public class WavWriter
    {
        /// <summary>
        /// If we're writing wav data, this is a flag for whether we've written the wav header.
        /// </summary>
        private bool wroteWavHeader;
        /// <summary>
        /// If we're writing wav data, this is the file path.
        /// </summary>
        private readonly string path;
        /// <summary>
        /// The number of channels.
        /// </summary>
        private readonly int channels;
        /// <summary>
        /// If we're writing wav data, this is a cached byte array for copying the audio float array into.
        /// </summary>
        private byte[] wavChunk = new byte[2048 * 4];
        /// <summary>
        /// A header for a .wav file.
        /// </summary>
        private static byte[] wavHeader;
        
        
        /// <summary>
        /// (constructor)
        /// </summary>
        /// <param name="path">The path to the output file.</param>
        /// <param name="overwrite">If true, overwrite an existing file.</param>
        /// <param name="channels">The number of audio channels.</param>
        public WavWriter(string path, bool overwrite = true, int channels = 1)
        {
            this.path = path;
            this.channels = channels;
            // Create the directory.
            string d = Path.GetDirectoryName(Path.GetFullPath(this.path));
            if (!Directory.Exists(d))
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                Directory.CreateDirectory(d);
            }
            if (overwrite && File.Exists(this.path))
            {
                File.Delete(this.path);
            }
        }


        /// <summary>
        /// Write audio samples to the .wav file.
        /// </summary>
        /// <param name="data">The audio data.</param>
        /// <param name="length">The length of the data chunk (this might be less than data.Length).</param>
        public void Write(float[] data, int length)
        {
            // Resize the wav data array.
            if (wavChunk.Length != length * 2)
            {
                Array.Resize(ref wavChunk, length * 2);
            }
            // Convert floats to int16 and copy the byte data into the array.
            // Source: https://gist.github.com/darktable/2317063
            for (int i = 0; i < length; i++)
            {
                Buffer.BlockCopy(BitConverter.GetBytes((short)(data[i] * Globals.FLOAT_TO_SHORT)), 0, wavChunk, i * 2, 2);
            }
            // Write.
            Write(wavChunk);
        }
        
        
        /// <summary>
        /// Write audio samples to the .wav file.
        /// </summary>
        /// <param name="data">The int16 byte array.</param>
        public void Write(byte[] data)
        {
            if (wavHeader == null)
            {
                SetWavHeader(channels);
            }
            // Write the .wav header.
            if (!wroteWavHeader)
            {
                wroteWavHeader = true;
                using (FileStream filestream = new FileStream(path, FileMode.Create))
                {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    // ReSharper disable once PossibleNullReferenceException
                    filestream.Write(wavHeader, 0, wavHeader.Length);
                }
            }
            // Write the byte array.
            using (FileStream filestream = new FileStream(path, FileMode.Append))
            {
                filestream.Write(data, 0, data.Length);
            }
        }


        /// <summary>
        /// End the wav file. Open the file and set metadata about data size. Source: https://docs.fileformat.com/audio/wav/
        /// </summary>
        public void End()
        {
            int fileSize = (int)new FileInfo(path).Length;
            using (FileStream filestream = new FileStream(path, FileMode.Open))
            {
                // Set the file size. 
                filestream.Seek(4, SeekOrigin.Begin);
                filestream.Write(BitConverter.GetBytes(fileSize), 0, 4);
                // Set the data size.
                filestream.Seek(40, SeekOrigin.Begin);
                filestream.Write(BitConverter.GetBytes(fileSize - wavHeader.Length), 0, 4);
            }
        }


        /// <summary>
        /// Set the wav header. Source: https://docs.fileformat.com/audio/wav/
        /// </summary>
        /// <param name="channels">The number of channels.</param>
        private static void SetWavHeader(int channels)
        {
            wavHeader = new byte[44];
            Buffer.BlockCopy(System.Text.Encoding.ASCII.GetBytes("RIFF"), 0, wavHeader, 0, 4);
            Buffer.BlockCopy(System.Text.Encoding.ASCII.GetBytes("WAVE"), 0, wavHeader, 8, 4);
            Buffer.BlockCopy(System.Text.Encoding.ASCII.GetBytes("fmt "), 0, wavHeader, 12, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(16), 0, wavHeader, 16, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((ushort)1), 0, wavHeader, 20, 2);
            Buffer.BlockCopy(BitConverter.GetBytes((ushort)channels), 0, wavHeader, 22, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(Globals.framerateInt), 0, wavHeader, 24, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(Globals.framerateInt * channels * 2), 0, wavHeader, 28, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((ushort)(channels * 2)), 0, wavHeader, 32, 2);
            Buffer.BlockCopy(BitConverter.GetBytes((ushort)16), 0, wavHeader, 34, 2);
            Buffer.BlockCopy(System.Text.Encoding.ASCII.GetBytes("data"), 0, wavHeader, 36, 4);
        }
    }
}