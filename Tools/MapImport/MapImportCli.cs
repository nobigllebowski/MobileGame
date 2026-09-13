// Command-line runner for the map import pipeline. Uses the same Nation.Core code as the Unity Editor menu.
// Build: see run.sh. Usage: MapImportCli <repo root>
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Nation.Core.Countries;
using Nation.Core.Data;
using Nation.Core.Map;
using Nation.Core.Map.Import;

public static class MapImportCli
{
    public static int Main(string[] args)
    {
        var root = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
        var countriesPath = Path.Combine(root, "Tools", "MapSource", "ne_50m_admin_0_countries.geojson");
        var placesPath = Path.Combine(root, "Tools", "MapSource", "ne_50m_populated_places.geojson");
        var simulationPath = Path.Combine(root, "Assets", "Data", "Countries", "countries.json");

        var stopwatch = Stopwatch.StartNew();
        var result = MapBuildPipeline.Build(File.ReadAllText(countriesPath), File.ReadAllText(placesPath), new MapBuildOptions());
        var buildMs = stopwatch.ElapsedMilliseconds;

        var simulation = new CountryCatalog(CountryDataParser.Parse(File.ReadAllText(simulationPath)));
        MapValidator.ValidateAgainstSimulation(result.Catalog, simulation, result.Report);

        var bytes = MapCatalogSerializer.Write(result.Catalog);
        var mapDirectory = Path.Combine(root, "Assets", "Data", "Map");
        Directory.CreateDirectory(mapDirectory);
        File.WriteAllBytes(Path.Combine(mapDirectory, "world.map.bytes"), bytes);

        var localizationDirectory = Path.Combine(root, "Assets", "Data", "Localization");
        foreach (var table in result.NameTables)
        {
            File.WriteAllText(Path.Combine(localizationDirectory, "map-names." + table.Key + ".json"), ToJson(table.Key, table.Value), new UTF8Encoding(false));
        }

        var report = new StringBuilder();
        report.Append("Natural Earth import report\n");
        report.Append("Generated: deterministic build from Tools/MapSource (see README)\n");
        report.Append("Countries: ").Append(result.Catalog.Countries.Count).Append('\n');
        report.Append("Source vertices: ").Append(result.SourceVertices).Append('\n');
        for (var band = 0; band < 3; band++)
        {
            var kind = (MapZoomBand)band;
            report.Append(kind).Append(": ").Append(result.Catalog.TotalVertices(kind)).Append(" vertices, ").Append(result.Catalog.TotalTriangles(kind)).Append(" triangles\n");
        }

        report.Append("Catalog size: ").Append(bytes.Length).Append(" bytes\n");
        report.Append("Build time: ").Append(buildMs).Append(" ms\n\n");
        report.Append(result.Report.Summary());
        File.WriteAllText(Path.Combine(mapDirectory, "import-report.txt"), report.ToString());

        Console.Write(report.ToString());
        return result.Report.HasErrors ? 1 : 0;
    }

    private static string ToJson(string locale, System.Collections.Generic.Dictionary<string, string> strings)
    {
        var keys = new System.Collections.Generic.List<string>(strings.Keys);
        keys.Sort(string.CompareOrdinal);
        var builder = new StringBuilder();
        builder.Append("{\n  \"locale\": \"").Append(locale).Append("\",\n  \"generated\": \"Natural Earth names; do not edit by hand, re-run the map import\",\n  \"strings\": {\n");
        for (var i = 0; i < keys.Count; i++)
        {
            builder.Append("    \"").Append(Escape(keys[i])).Append("\": \"").Append(Escape(strings[keys[i]])).Append('"');
            builder.Append(i < keys.Count - 1 ? ",\n" : "\n");
        }

        builder.Append("  }\n}\n");
        return builder.ToString();
    }

    private static string Escape(string value)
    {
        return value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n");
    }
}
