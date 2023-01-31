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
            {"--type [STRING]", "The type of audio event. Options: impact, scrape"},
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
            {"--path [STRING]", "OPTIONAL. The path to a .wav file. If not included, audio will be written to stdout."},
            {"--min_speed [FLOAT]", "OPTIONAL. If included, set the minimum speed. If the speed is slower than this, don't generate audio. See: AudioGenerator.minSpeed"},
            {"--allow_distortion", "OPTIONAL. If included, don't clamp impact amp values to 0.99. See: Impact.preventDistortion"},
            {"--unclamp_contact_time", "OPTIONAL. If included, don't clamp impact contact times to plausible values. See: Impact.clampContactTime"},
            {"--scrape_max_speed [FLOAT]", "OPTIONAL. Clamp scrape speeds to this maximum value. See: Scrape.maxSpeed"},
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
            AudioEvent.simulationAmp = AudioEvent.simulationAmp.Clamp(0, 1);
            ArgumentParser.TryGetBooleanValue(args, "allow_distortion", ref Impact.preventDistortion);
            ArgumentParser.TryGetBooleanValue(args, "unclamp_contact_time", ref Impact.clampContactTime);
            ArgumentParser.TryGetDoubleValue(args, "scrape_max_speed", ref Scrape.maxSpeed);
            // Set the primary object.
            ClatterObjectData primary = GetClatterObjectData(args, 0, "primary", false);
            // Get the audio type.
            string audioType = ArgumentParser.GetStringValue(args, "type");
            // Check if this is a scrape.
            bool scrape;
            double scrapeDuration;
            if (audioType == "impact")
            {
                scrape = false;
                scrapeDuration = -1;
            }
            else if (audioType == "scrape")
            {
                scrape = true;
                scrapeDuration = ArgumentParser.GetDoubleValue(args, "duration");
            }
            else
            {
                throw new Exception("Invalid audio type: " + audioType);
            }
            // Set the secondary object.
            ClatterObjectData secondary = GetClatterObjectData(args, 1, "secondary", scrape);
            // Get the speed.
            double speed = ArgumentParser.GetDoubleValue(args, "speed");
            // Write a wav file.
            string path = "";
            if (ArgumentParser.TryGetStringValue(args, "path", ref path))
            {
                if (scrape)
                {
                    WriteScrape(primary, secondary, speed, scrapeDuration, path);
                }
                else
                {
                    WriteImpact(primary, secondary, speed, path);
                }     
            }
            // Generate data and write to standard output.
            else
            {
                byte[] audio;
                if (scrape)
                {
                    audio = GetScrape(primary, secondary, speed, scrapeDuration);
                }
                else
                {
                    audio = GetImpact(primary, secondary, speed);
                }
                using (Stream stream = Console.OpenStandardOutput())
                {
                    stream.Write(audio, 0, audio.Length);
                }
            }
        }
        

        /// <summary>
        /// Return a Clatter object.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <param name="id">The object ID.</param>
        /// <param name="target">Either primary or secondary.</param>
        /// <param name="scrape">If true, look for a scrape material.</param>
        private static ClatterObjectData GetClatterObjectData(string[] args, uint id, string target, bool scrape)
        {
            string m = ArgumentParser.GetStringValue(args, target + "_material");
            ImpactMaterial impactMaterial;
            if (!Enum.TryParse(m, out impactMaterial))
            {
                throw new Exception("Invalid impact material: " + m);
            }
            ImpactMaterialData.Load(impactMaterial);
            double amp = ArgumentParser.GetDoubleValue(args, target + "_amp");
            double resonance = ArgumentParser.GetDoubleValue(args, target + "_resonance");
            double mass = ArgumentParser.GetDoubleValue(args, target + "_mass");
            if (scrape)
            {
                string s = ArgumentParser.GetStringValue(args, "scrape_material");
                ScrapeMaterial scrapeMaterial;
                if (!Enum.TryParse(s, out scrapeMaterial))
                {
                    throw new Exception("Invalid scrape material: " + s);
                }
                ScrapeMaterialData.Load(scrapeMaterial);
                return new ClatterObjectData(id, impactMaterial, amp, resonance, mass, scrapeMaterial);
            }
            else
            {
                return new ClatterObjectData(id, impactMaterial, amp, resonance, mass);
            }
        }


        /// <summary>
        /// Generate an impact sound.
        /// This assumes that you have called SetPrimaryObject and SetSecondaryObject.
        /// </summary>
        /// <param name="primary">The primary object.</param>
        /// <param name="secondary">The secondary object.</param>
        /// <param name="speed">The speed of the impact.</param>
        private static byte[] GetImpact(ClatterObjectData primary, ClatterObjectData secondary, double speed)
        {
            Impact impact = new Impact(primary, secondary, new Random());
            // Generate audio.
            bool ok = impact.GetAudio(speed);
            if (!ok)
            {
                return new byte[0];
            }
            else
            {
                return impact.samples.ToInt16Bytes();
            }
        }
        
        
        /// <summary>
        /// Generate a scrape sound.
        /// This assumes that you have called SetPrimaryObject and SetSecondaryObject, and that in the latter call you provided a scrape material.
        /// </summary>
        /// <param name="primary">The primary object.</param>
        /// <param name="secondary">The secondary object.</param>
        /// <param name="speed">The speed of the scrape.</param>
        /// <param name="duration">The duration of the scrape in seconds. This will be rounded to the nearest tenth of a second.</param>
        private static byte[] GetScrape(ClatterObjectData primary, ClatterObjectData secondary, double speed, double duration)
        {
            // Get the number of scrape events.
            int count = Scrape.GetNumScrapeEvents(duration);
            // Get the scrape material.
            primary.speed = speed;
            // Get the scrape.
            Scrape scrape = new Scrape(secondary.scrapeMaterial, primary, secondary, new Random());
            byte[] audio = new byte[Scrape.SAMPLES_LENGTH * 2 * count];
            int c = Scrape.SAMPLES_LENGTH * 2;
            for (int i = 0; i < count; i++)
            {
                // Continue the scrape.
                scrape.GetAudio(speed);
                // Get the audio and copy it to the buffer.
                Buffer.BlockCopy(scrape.samples.ToInt16Bytes(), 0, audio, i * c, c);
            }
            return audio;
        }
        
        
        /// <summary>
        /// Generate an impact sound and write it to disk as a .wav file.
        /// </summary>
        /// <param name="primary">The primary object.</param>
        /// <param name="secondary">The secondary object.</param>
        /// <param name="speed">The speed of the impact.</param>
        /// <param name="path">The path to the output file.</param>
        private static void WriteImpact(ClatterObjectData primary, ClatterObjectData secondary, double speed, string path)
        {
            WavWriter writer = new WavWriter(path);
            writer.Write(GetImpact(primary, secondary, speed));
            writer.End();
        }
        
        
        /// <summary>
        /// Generate a scrape sound and write it to disk as a .wav file.
        /// This assumes that you have called SetPrimaryObject and SetSecondaryObject, and that in the latter call you provided a scrape material.
        /// </summary>
        /// <param name="primary">The primary object.</param>
        /// <param name="secondary">The secondary object.</param>
        /// <param name="speed">The speed of the scrape.</param>
        /// <param name="duration">The duration of the scrape in seconds. This will be rounded to the nearest tenth of a second.</param>
        /// <param name="path">The path to the output file.</param>
        private static void WriteScrape(ClatterObjectData primary, ClatterObjectData secondary, double speed, double duration, string path)
        {
            WavWriter writer = new WavWriter(path);
            writer.Write(GetScrape(primary, secondary, speed, duration));
            writer.End();
        }
    }   
}
