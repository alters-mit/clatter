using System;
using System.IO;
using System.Reflection;


namespace Clatter.Core
{
    /// <summary>
    /// Load files embedded in the assembly.
    /// </summary>
    public static class Loader
    {
        /// <summary>
        /// Conversion factor for int16 to double.
        /// </summary>
        private const double INT16_TO_DOUBLE = 32767;
        

        /// <summary>
        /// This assembly.
        /// </summary>
        private static readonly Assembly ExecutingAssembly = Assembly.Load("Clatter.Core");


        /// <summary>
        /// Load a binary file from the assembly.
        /// </summary>
        /// <param name="pathFromData">The path to the file relative to Data.</param>
        public static byte[] Load(string pathFromData)
        {
            // Load the file. Source: https://stackoverflow.com/a/38514563
            using (Stream stream = ExecutingAssembly.GetManifestResourceStream("Clatter.Core.Data." + pathFromData))
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    byte[] buffer = new byte[stream.Length];
                    // ReSharper disable once MustUseReturnValue
                    reader.Read(buffer, 0, buffer.Length);
                    return buffer;         
                }
            }
        }
        
        
        /// <summary>
        /// Load impulse response data from a file. 
        /// </summary>
        /// <param name="path">The absolute file path.</param>
        public static double[] LoadImpulseResponse(string path)
        {
            // Load the .wav file.
            byte[] irb = File.ReadAllBytes(path);
            
            // Get the number of channels.
            short numChannels = BitConverter.ToInt16(irb, 22);
            
            // Get the number of samples (not bytes).
            int numSamples = (irb.Length - 44) / 2 / numChannels;
            double[] ir = new double[numSamples];

            int step = 2 * numChannels;
            
            // Read each sample.
            double sample;
            int irIndex = 0;
            for (int i = 44; i < irb.Length; i += step)
            {
                // One channel.
                if (numChannels == 1)
                {
                    // Read the sample.
                    ir[irIndex] = BitConverter.ToInt16(irb, i) / INT16_TO_DOUBLE;
                }
                // Overlay each channel.
                else
                {
                    sample = 0;
                    for (int j = i; j < i + step; j += 2)
                    {
                        sample += BitConverter.ToInt16(irb, j) / INT16_TO_DOUBLE;
                        if (sample < -1)
                        {
                            sample = -1;
                            break;
                        }
                        else if (sample > 1)
                        {
                            sample = 1;
                            break;
                        }
                    }
                    ir[irIndex] = sample;
                }
                irIndex++;
            }
            return ir;
        }
    }
}