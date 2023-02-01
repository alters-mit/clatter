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
                if (ArgIsFlag(args[i], flag))
                {
                    return args[i + 1];
                }
            }
            return string.Empty;
        }


        /// <summary>
        /// Returns the double value of a flag argument.
        /// Example: If the argument is `--mass 0.1`, then `flag` should be `"mass"` and this should return `0.1`.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <param name="flag">The flag preceding the value without the `"--"` prefix.</param>
        public static double GetDoubleValue(string[] args, string flag)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (ArgIsFlag(args[i], flag))
                {
                    double d;
                    if (double.TryParse(args[i + 1], out d))
                    {
                        return d;
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
        /// Try to get a string value from an optional flag. Returns true if the flag is present.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <param name="flag">The flag preceding the value without the `"--"` prefix.</param>
        /// <param name="value">The value.</param>
        public static bool TryGetStringValue(string[] args, string flag, ref string value)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (ArgIsFlag(args[i], flag))
                {
                    value = args[i + 1];
                    return true;
                }
            }
            return false;
        }
        
        
        /// <summary>
        /// Try to get a double value from an optional flag. If the flag isn't present, the value isn't set.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <param name="flag">The flag preceding the value without the `"--"` prefix.</param>
        /// <param name="value">The value.</param>
        public static bool TryGetDoubleValue(string[] args, string flag, ref double value)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (ArgIsFlag(args[i], flag))
                {
                    double v;
                    if (double.TryParse(args[i + 1], out v))
                    {
                        value = v;
                        return true;
                    }
                }
            }
            return false;
        }
        
        
        /// <summary>
        /// Try to get an int value from an optional flag. If the flag isn't present, the value isn't set.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <param name="flag">The flag preceding the value without the `"--"` prefix.</param>
        /// <param name="value">The value.</param>
        public static bool TryGetIntValue(string[] args, string flag, ref int value)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (ArgIsFlag(args[i], flag))
                {
                    int v;
                    if (int.TryParse(args[i + 1], out v))
                    {
                        value = v;
                        return true;
                    }
                }
            }
            return false;
        }
        
        
        /// <summary>
        /// Try to get a boolean value from an optional flag. If the flag isn't present, the value isn't set.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <param name="flag">The flag preceding the value without the `"--"` prefix.</param>
        /// <param name="value">The value.</param>
        public static bool TryGetBooleanValue(string[] args, string flag, ref bool value)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (ArgIsFlag(args[i], flag))
                {
                    value = !value;
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Returns true if this argument is the flag.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="flag">The flag.</param>
        private static bool ArgIsFlag(string arg, string flag)
        {
            return arg.StartsWith("--") && arg.Substring(2) == flag;
        }
    }
}