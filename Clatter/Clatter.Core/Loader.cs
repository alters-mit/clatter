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
        /// This assembly. Source: https://stackoverflow.com/a/70627269
        /// </summary>
        // ReSharper disable once AssignNullToNotNullAttribute
        private static readonly Assembly ExecutingAssembly = Assembly.LoadFrom(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Clatter.Core.dll"));


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
    }
}