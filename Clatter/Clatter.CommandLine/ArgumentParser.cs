namespace Clatter.CommandLine
{
    /// <summary>
    /// A basic argument parser.
    /// </summary>
    public static class ArgumentParser
    {
        /// <summary>
        /// Returns the command line argument for a given flag.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <param name="flag">The flag preceding the value.</param>
        public static string GetStringValue(string[] args, string flag)
        {
            for (int i = 0; i < args.Length; i++)
            {
                // This is the flag.
                if (args[i].StartsWith("--") && args[i].Substring(3) == flag)
                {
                    return args[i + 1];
                }
            }
            return string.Empty;
        }
        
        
        /// <summary>
        /// Returns the command line argument for a given flag.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <param name="flag">The flag preceding the value.</param>
        public static float GetFloatValue(string[] args, string flag)
        {
            for (int i = 0; i < args.Length; i++)
            {
                // This is the flag.
                if (args[i].StartsWith("--") && args[i].Substring(3) == flag)
                {
                    float f;
                    if (float.TryParse(args[i + 1], out f))
                    {
                        return f;
                    }
                    else
                    {
                        throw new Exception("Invalid float: --" + flag);
                    }
                }
            }
            return 0;
        }
    }
}