using Clatter.Core;


namespace Clatter.CommandLine
{
    /// <summary>
    /// A command-line program for generating audio.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// A cached impact material.
        /// </summary>
        private static ImpactMaterialSized impactMaterial;
        /// <summary>
        /// A cached scrape material,
        /// </summary>
        private static ScrapeMaterial scrapeMaterial;

        
        private static void Main(string[] args)
        {
            // Get the path to the output file.
            string path = ArgumentParser.GetStringValue(args, "path");
            // Set the primary object.
            Creator.SetPrimaryObject(GetAudioObjectData(args, 0, "primary", false));
            // Get the audio type.
            string audioType = ArgumentParser.GetStringValue(args, "type");
            // Check if this is a scrape.
            bool scrape;
            if (audioType == "impact")
            {
                scrape = false;
            }
            else if (audioType == "scrape")
            {
                scrape = true;
            }
            else
            {
                throw new Exception("Invalid audio type: " + audioType);
            }
            // Set the secondary object.
            Creator.SetSecondaryObject(GetAudioObjectData(args, 1, "secondary", scrape));
            float speed = ArgumentParser.GetFloatValue(args, "speed");
            if (scrape)
            {
                Creator.WriteScrape(speed, ArgumentParser.GetFloatValue(args, "duration"), path);
            }
            else
            {
                Creator.WriteImpact(speed, true, path);
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
            if (!Enum.TryParse(m, out impactMaterial))
            {
                throw new Exception("Invalid impact material: " + m);
            }
            float amp = ArgumentParser.GetFloatValue(args, target + "_amp");
            float resonance = ArgumentParser.GetFloatValue(args, target + "_resonance");
            float mass = ArgumentParser.GetFloatValue(args, target + "_mass");
            if (scrape)
            {
                string s = ArgumentParser.GetStringValue(args, "scrape_material");
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
