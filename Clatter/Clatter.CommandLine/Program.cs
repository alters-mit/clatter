using Clatter.Core;


namespace Clatter.CommandLine
{
    /// <summary>
    /// A command-line program for generating audio.
    /// </summary>
    public class Program
    {
        private static void Main(string[] args)
        {
            // Set static values.
            ArgumentParser.TryGetDoubleValue(args, "simulation_amp", ref AudioEvent.simulationAmp);
            ArgumentParser.TryGetBooleanValue(args, "allow_distortion", ref AudioEvent.preventDistortion);
            ArgumentParser.TryGetBooleanValue(args, "clamp_contact_time", ref AudioEvent.clampContactTime);
            ArgumentParser.TryGetDoubleValue(args, "scrape_max_speed", ref Scrape.scrapeMaxSpeed);
            // Set the primary object.
            Creator.SetPrimaryObject(GetAudioObjectData(args, 0, "primary", false));
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
            Creator.SetSecondaryObject(GetAudioObjectData(args, 1, "secondary", scrape));
            // Get the speed.
            double speed = ArgumentParser.GetDoubleValue(args, "speed");
            // Get the path to the output file.
            string path;
            // Write a file.
            if (ArgumentParser.TryGetStringValue(args, "path", out path))
            {
                if (scrape)
                {
                    Creator.WriteScrape(speed, scrapeDuration, path);
                }
                else
                {
                    Creator.WriteImpact(speed, true, path);
                }         
            }
            // Write to stdout.
            else
            {
                // Get the audio data.
                byte[] buffer;
                if (scrape)
                {
                    buffer = Creator.GetScrape(speed, scrapeDuration);
                }
                else
                {
                    buffer = Creator.GetImpact(speed, true);
                }
                // Write the audio data. Source: https://stackoverflow.com/a/27007411
                using (Stream outStream = Console.OpenStandardOutput())
                {
                    if (scrape)
                    {
                        outStream.Write(buffer, 0, buffer.Length);
                    }
                }
            }
        }
        

        /// <summary>
        /// Returns object audio data.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <param name="id">The object ID.</param>
        /// <param name="target">Either primary or secondary.</param>
        /// <param name="scrape">If true, look for a scrape material.</param>
        private static AudioObjectData GetAudioObjectData(string[] args, uint id, string target, bool scrape)
        {
            string m = ArgumentParser.GetStringValue(args, target + "_material") + "_" + ArgumentParser.GetStringValue(args, target + "_size");
            ImpactMaterial impactMaterial;
            if (!Enum.TryParse(m, out impactMaterial))
            {
                throw new Exception("Invalid impact material: " + m);
            }
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
                return new AudioObjectData(id, impactMaterial, amp, resonance, mass, scrapeMaterial);
            }
            else
            {
                return new AudioObjectData(id, impactMaterial, amp, resonance, mass);
            }
        }
    }   
}
