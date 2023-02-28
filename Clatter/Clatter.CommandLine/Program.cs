using Clatter.Core;


namespace Clatter.CommandLine
{
    /// <summary>
    /// A command-line program for generating audio using Clatter.
    /// </summary>
    public class Program
    {
        private const string HELP_HEADER = "A command-line program for generating audio using Clatter.\n\n" +
                                           "EXAMPLE CALL:\n\n" +
                                           "./clatter.exe --primary_material glass_1 --primary_amp 0.2 --primary_resonance 0.2 --primary_mass 1 --secondary_material stone_4 --secondary_amp 0.5 --secondary_resonance 0.1 --secondary_mass 100 --speed 1 --type impact --path out.wav\n\n" +
                                           "ARGUMENTS:\n\n";
        /// <summary>
        /// Help text per argument.
        /// </summary>
        private static readonly Dictionary<string, string> ArgsHelp = new()
        {
            {"--primary_material [STRING]", "The primary object's ImpactMaterial. See ImpactMaterial API documentation for a list of options."},
            {"--primary_amp [FLOAT]", "The primary object's amp value (0-1)."},
            {"--primary_resonance [FLOAT]", "The primary object's resonance value (0-1)."},
            {"--primary_mass [FLOAT]", "The primary object's mass."},
            {"--secondary_material [STRING]", "The secondary object's ImpactMaterial. See ImpactMaterial API documentation for a list of options."},
            {"--secondary_amp [FLOAT]", "The secondary object's amp value (0-1)."},
            {"--secondary_resonance [FLOAT]", "The secondary object's resonance value (0-1)."},
            {"--secondary_mass [FLOAT]", "The secondary object's mass."},
            {"--speed [FLOAT]", "The speed of the collision."},
            {"--scrape_material [STRING]", "If --type is scrape, this sets the secondary object's scrape map. See ScrapeMaterial API documentation for a list of options."},
            {"--duration [FLOAT]", "If --type is scrape, this sets the duration of the scrape audio."},
            {"--simulation_amp [FLOAT]", "The overall amp (0-1)."},
            {"--type [STRING]", "The type of audio event. Options: impact, scrape"},
            {"--path [STRING]", "OPTIONAL. The path to a .wav file. If not included, audio will be written to stdout."},
            {"--allow_distortion", "OPTIONAL. If included, don't clamp impact amp values to 0.99. See: Impact.preventDistortion"},
            {"--unclamp_contact_time", "OPTIONAL. If included, don't clamp impact contact times to plausible values. See: Impact.clampContactTime"},
            {"--scrape_max_speed [FLOAT]", "OPTIONAL. Clamp scrape speeds to this maximum value. See: Scrape.maxSpeed"},
            {"--framerate [INT]", "OPTIONAL. The audio framerate. If not included, defaults to 44100. See: Globals.framerate"},
            {"--help", "OPTIONAL. Print this message and exit."}
        };
        
        
        private static void Main(string[] args)
        {
            // Print the help text and end.
            bool help = false;
            ArgumentParser.TryGetBooleanValue(args, "help", ref help);
            if (help || args.Length == 0)
            {
                string helpText = HELP_HEADER;
                foreach (string key in ArgsHelp.Keys)
                {
                    helpText += key + ": " + ArgsHelp[key] + "\n\n";
                }
                Console.WriteLine(helpText.Trim());
                return;
            }
            // Set static values.
            ArgumentParser.TryGetDoubleValue(args, "simulation_amp", ref AudioEvent.simulationAmp);
            AudioEvent.simulationAmp = AudioEvent.simulationAmp.Clamp(0, 0.99);
            ArgumentParser.TryGetBooleanValue(args, "allow_distortion", ref Impact.preventDistortion);
            ArgumentParser.TryGetBooleanValue(args, "unclamp_contact_time", ref Impact.clampContactTime);
            ArgumentParser.TryGetDoubleValue(args, "scrape_max_speed", ref Scrape.maxSpeed);
            if (ArgumentParser.TryGetIntValue(args, "framerate", ref Globals.framerate))
            {
                Globals.framerateD = Globals.framerate;
            }
            // Get the primary object's data.
            ImpactMaterial primaryImpactMaterial;
            double primaryAmp;
            double primaryResonance;
            double primaryMass;
            GetClatterObjectData(args, "primary", out primaryImpactMaterial, out primaryAmp, out primaryResonance, out primaryMass);
            // Get the secondary object's data.
            ImpactMaterial secondaryImpactMaterial;
            double secondaryAmp;
            double secondaryResonance;
            double secondaryMass;
            GetClatterObjectData(args, "secondary", out secondaryImpactMaterial, out secondaryAmp, out secondaryResonance, out secondaryMass);
            // Get the audio type.
            string audioEventTypeStr = ArgumentParser.GetStringValue(args, "type");
            AudioEventType audioEventType;
            // Check if this is a scrape.
            double scrapeDuration;
            ScrapeMaterial scrapeMaterial;
            if (audioEventTypeStr == "impact")
            {
                audioEventType = AudioEventType.impact;
                scrapeDuration = -1;
                scrapeMaterial = default;
            }
            else if (audioEventTypeStr == "scrape")
            {
                audioEventType = AudioEventType.scrape;
                scrapeDuration = ArgumentParser.GetDoubleValue(args, "duration");
                string s = ArgumentParser.GetStringValue(args, "scrape_material");
                if (!Enum.TryParse(s, out scrapeMaterial))
                {
                    throw new Exception("Invalid scrape material: " + s);
                }
                ScrapeMaterialData.Load(scrapeMaterial);
            }
            else
            {
                throw new Exception("Invalid audio type: " + audioEventTypeStr);
            }
            // Get the speed.
            double speed = ArgumentParser.GetDoubleValue(args, "speed");
            // Generate audio.
            byte[] audio = ExternalEntryPoint.GetAudio((byte)primaryImpactMaterial, primaryAmp, primaryResonance, primaryMass, 
                (byte)secondaryImpactMaterial, secondaryAmp, secondaryResonance, secondaryMass, speed, 
                (byte)audioEventType, (byte)scrapeMaterial, scrapeDuration, false, 0, 
                AudioEvent.simulationAmp,  Scrape.maxSpeed, Impact.preventDistortion, Impact.clampContactTime,
                Globals.framerate);
            // Write a wav file.
            string path = "";
            if (ArgumentParser.TryGetStringValue(args, "path", ref path))
            {
                WavWriter writer = new WavWriter(path);
                writer.Write(audio);
                writer.End();
            }
            // Generate data and write to standard output.
            else
            {
                using (Stream stream = Console.OpenStandardOutput())
                {
                    stream.Write(audio, 0, audio.Length);
                }
            }
        }


        /// <summary>
        /// Get Clatter object data.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <param name="target">Either primary or secondary.</param>
        /// <param name="impactMaterial">The impact material.</param>
        /// <param name="amp">The amp value.</param>
        /// <param name="resonance">The resonance value.</param>
        /// <param name="mass">The mass value.</param>
        private static void GetClatterObjectData(string[] args, string target, out ImpactMaterial impactMaterial, out double amp, out double resonance, out double mass)
        {
            string m = ArgumentParser.GetStringValue(args, target + "_material");
            if (!Enum.TryParse(m, out impactMaterial))
            {
                throw new Exception("Invalid impact material: " + m);
            }
            ImpactMaterialData.Load(impactMaterial);
            amp = ArgumentParser.GetDoubleValue(args, target + "_amp");
            resonance = ArgumentParser.GetDoubleValue(args, target + "_resonance");
            mass = ArgumentParser.GetDoubleValue(args, target + "_mass");
        }
    }   
}
