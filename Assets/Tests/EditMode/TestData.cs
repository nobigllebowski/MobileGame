using System.IO;

namespace Nation.Tests
{
    /// <summary>Reads the shipped data files from the project so tests cover the real content.</summary>
    internal static class TestData
    {
        private static string Root
        {
            get
            {
                var directory = Directory.GetCurrentDirectory();
                while (directory != null && !Directory.Exists(Path.Combine(directory, "Assets")))
                {
                    directory = Path.GetDirectoryName(directory);
                }

                return directory ?? Directory.GetCurrentDirectory();
            }
        }

        public static string Read(params string[] segments)
        {
            var path = Path.Combine(Root, "Assets", "Data");
            foreach (var segment in segments)
            {
                path = Path.Combine(path, segment);
            }

            return File.ReadAllText(path);
        }

        public static byte[] ReadBytes(params string[] segments)
        {
            var path = Path.Combine(Root, "Assets", "Data");
            foreach (var segment in segments)
            {
                path = Path.Combine(path, segment);
            }

            return File.ReadAllBytes(path);
        }

        public static string Countries => Read("Countries", "countries.json");
        public static string Buildings => Read("Buildings", "buildings.json");
        public static string Map => Read("Map", "continents.json");
        public static string English => Read("Localization", "en.json");
        public static string Russian => Read("Localization", "ru.json");
    }
}
