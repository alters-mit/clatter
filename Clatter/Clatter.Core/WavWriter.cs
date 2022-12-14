using System;
using System.IO;


namespace Clatter.Core
{
    /// <summary>
    /// Write audio samples to a .wav file.
    /// </summary>
    public class WavWriter
    {
        /// <summary>
        /// If we're writing wav data, this is a flag for whether we've written the wav header.
        /// </summary>
        private bool _wroteWavHeader;
        /// <summary>
        /// If we're writing wav data, this is the file path.
        /// </summary>
        private readonly string _path;
        /// <summary>
        /// If we're writing wav data, this is a cached byte array for copying the audio float array into.
        /// </summary>
        private byte[] _wavChunk = new byte[2048 * 4];
        /// <summary>
        /// A header for a .wav file.
        /// </summary>
        private static byte[] _wavHeader;


        public WavWriter(string path, bool overwrite = true)
        {
            _path = path;
            // Create the directory.
            string d = Path.GetDirectoryName(_path);
            if (!Directory.Exists(d))
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                Directory.CreateDirectory(d);
            }
            if (overwrite && File.Exists(_path))
            {
                File.Delete(_path);
            }
        }


        /// <summary>
        /// Write audio samples as a .wav file.
        /// </summary>
        /// <param name="data">The audio data.</param>
        /// <param name="length">The length of the data chunk (this might be less than data.Length).</param>
        /// <param name="channels">The number of audio channels.</param>
        public void Write(float[] data, int length, int channels)
        {
            if (_wavHeader == null)
            {
                SetWavHeader(channels);
            }
            // Write the .wav header.
            if (!_wroteWavHeader)
            {
                _wroteWavHeader = true;
                using (FileStream filestream = new FileStream(_path, FileMode.Create))
                {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    // ReSharper disable once PossibleNullReferenceException
                    filestream.Write(_wavHeader, 0, _wavHeader.Length);
                }
            }
            // Resize the wav data array.
            if (_wavChunk.Length != length * 2)
            {
                Array.Resize(ref _wavChunk, length * 2);
            }
            // Convert floats to int16 and copy the byte data into the array.
            // Source: https://gist.github.com/darktable/2317063
            for (int i = 0; i < length; i++)
            {
                Buffer.BlockCopy(BitConverter.GetBytes((short)(data[i] * Globals.FLOAT_TO_SHORT)), 0, _wavChunk, i * 2, 2);
            }
            // Write the byte array.
            using (FileStream filestream = new FileStream(_path, FileMode.Append))
            {
                filestream.Write(_wavChunk, 0, _wavChunk.Length);
            }
        }


        /// <summary>
        /// End the wav file. Open the file and set metadata about data size. Source: https://docs.fileformat.com/audio/wav/
        /// </summary>
        public void End()
        {
            int fileSize = (int)new FileInfo(_path).Length;
            using (FileStream filestream = new FileStream(_path, FileMode.Open))
            {
                // Set the file size. 
                filestream.Seek(4, SeekOrigin.Begin);
                filestream.Write(BitConverter.GetBytes(fileSize), 0, 4);
                // Set the data size.
                filestream.Seek(40, SeekOrigin.Begin);
                filestream.Write(BitConverter.GetBytes(fileSize - _wavHeader.Length), 0, 4);
            }
        }


        /// <summary>
        /// Set the wav header. Source: https://docs.fileformat.com/audio/wav/
        /// </summary>
        /// <param name="channels">The number of channels.</param>
        private static void SetWavHeader(int channels)
        {
            _wavHeader = new byte[44];
            Buffer.BlockCopy(System.Text.Encoding.ASCII.GetBytes("RIFF"), 0, _wavHeader, 0, 4);
            Buffer.BlockCopy(System.Text.Encoding.ASCII.GetBytes("WAVE"), 0, _wavHeader, 8, 4);
            Buffer.BlockCopy(System.Text.Encoding.ASCII.GetBytes("fmt "), 0, _wavHeader, 12, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(16), 0, _wavHeader, 16, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((ushort)1), 0, _wavHeader, 20, 2);
            Buffer.BlockCopy(BitConverter.GetBytes((ushort)channels), 0, _wavHeader, 22, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(Globals.framerateInt), 0, _wavHeader, 24, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(Globals.framerateInt * channels * 2), 0, _wavHeader, 28, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((ushort)(channels * 2)), 0, _wavHeader, 32, 2);
            Buffer.BlockCopy(BitConverter.GetBytes((ushort)16), 0, _wavHeader, 34, 2);
            Buffer.BlockCopy(System.Text.Encoding.ASCII.GetBytes("data"), 0, _wavHeader, 36, 4);
        }
    }
}