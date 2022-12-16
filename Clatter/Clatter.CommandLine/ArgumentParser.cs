namespace Clatter.CommandLine
{
    /// <summary>
    /// A basic argument parser.
    /// </summary>
    public static class ArgumentParser
    {
        /// <summary>
        /// Returns the string value of a flag argument.
        /// Example: If the argument is `--name Cube`, then `flag` should be `"name"` and this should return `"Cube"`.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <param name="flag">The flag preceding the value without the `"--"` prefix.</param>
        public static string GetStringValue(string[] args, string flag)
        {
            for (int i = 0; i < args.Length; i++)
            {
                // This is the flag.
                if (args[i].StartsWith("--") && args[i].Substring(2) == flag)
                {
                    return args[i + 1];
                }
            }
            return string.Empty;
        }
        
        
        /// <summary>
        /// Try to get a string value from an optional flag.
        /// Example: If the argument is `--name Cube`, then `flag` should be `"name"`, `value` will be `"Cube"`, and this returns `true`.
        /// If `--name` isn't present in the command-line arguments, this returns `false`.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <param name="flag">The flag preceding the value without the `"--"` prefix.</param>
        /// <param name="value">The value.</param>
        public static bool TryGetStringValue(string[] args, string flag, out string value)
        {
            for (int i = 0; i < args.Length; i++)
            {
                // This is the flag.
                if (args[i].StartsWith("--") && args[i].Substring(2) == flag)
                {
                    value = args[i + 1];
                    return true;
                }
            }
            value = string.Empty;
            return false;
        }
        
        
        /// <summary>
        /// Returns the float value of a flag argument.
        /// Example: If the argument is `--mass 0.1`, then `flag` should be `"mass"` and this should return `0.1f`.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <param name="flag">The flag preceding the value without the `"--"` prefix.</param>
        public static float GetFloatValue(string[] args, string flag)
        {
            for (int i = 0; i < args.Length; i++)
            {
                // This is the flag.
                if (args[i].StartsWith("--") && args[i].Substring(2) == flag)
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
        
        
        /// <summary>
        /// Try to get a double value from an optional flag. If the flag isn't present, the value isn't set.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <param name="flag">The flag preceding the value without the `"--"` prefix.</param>
        /// <param name="value">The value.</param>
        public static void TryGetDoubleValue(string[] args, string flag, ref double value)
        {
            for (int i = 0; i < args.Length; i++)
            {
                // This is the flag.
                if (args[i].StartsWith("--") && args[i].Substring(2) == flag)
                {
                    double d;
                    if (double.TryParse(args[i + 1], out d))
                    {
                        value = d;
                        return;
                    }
                }
            }
        }
        
        
        /// <summary>
        /// Try to get a boolean value from an optional flag. If the flag isn't present, the value isn't set.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <param name="flag">The flag preceding the value without the `"--"` prefix.</param>
        /// <param name="value">The value.</param>
        public static void TryGetBooleanValue(string[] args, string flag, ref bool value)
        {
            for (int i = 0; i < args.Length; i++)
            {
                // This is the flag.
                if (args[i].StartsWith("--") && args[i].Substring(2) == flag)
                {
                    value = !value;
                    return;
                }
            }
        }
    }
}