using System.IO;
using System.Reflection;


namespace Clatter.Core
{
    /// <summary>
    /// Misc. file paths.
    /// </summary>
    public static class Paths
    {
        /// <summary>
        /// The name of the folder containing the scrape material data files.
        /// </summary>
        public const string SCRAPE_MATERIAL_DATA_FOLDER = "scrape_material_data/";
        /// <summary>
        /// The name of the folder containing the impact material data files.
        /// </summary>
        public const string IMPACT_MATERIAL_FOLDER = "impact_material_data/";


        /// <summary>
        /// The path to the root directory.
        /// </summary>
        public static string root = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    }
}