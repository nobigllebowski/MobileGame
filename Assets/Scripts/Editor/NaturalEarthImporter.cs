using System;
using System.IO;
using System.Text;
using Nation.Core.Countries;
using Nation.Core.Data;
using Nation.Core.Map;
using Nation.Core.Map.Import;
using UnityEditor;
using UnityEngine;

namespace Nation.Editor
{
    /// <summary>
    /// Editor front-end for the engine-free map pipeline. SOURCE GEO DATA → IMPORT → VALIDATE → PROCESS →
    /// GENERATE. Reads Tools/MapSource, writes the binary catalog, the generated name tables and the report.
    /// The command-line runner in Tools/MapImport calls the same MapBuildPipeline and produces identical bytes.
    /// </summary>
    public static class NaturalEarthImporter
    {
        private const string CountriesSource = "Tools/MapSource/ne_50m_admin_0_countries.geojson";
        private const string PlacesSource = "Tools/MapSource/ne_50m_populated_places.geojson";
        private const string SimulationData = "Assets/Data/Countries/countries.json";
        private const string CatalogOutput = "Assets/Data/Map/world.map.bytes";
        private const string ReportOutput = "Assets/Data/Map/import-report.txt";
        private const string LocalizationFolder = "Assets/Data/Localization";

        [MenuItem("Nation/Map/Import Natural Earth")]
        public static void Import()
        {
            var root = ProjectRoot();
            var countriesPath = Path.Combine(root, CountriesSource);
            var placesPath = Path.Combine(root, PlacesSource);
            if (!File.Exists(countriesPath) || !File.Exists(placesPath))
            {
                EditorUtility.DisplayDialog("Natural Earth import", "Source files not found under Tools/MapSource. See Tools/MapSource/README.md.", "OK");
                return;
            }

            try
            {
                EditorUtility.DisplayProgressBar("Natural Earth import", "Reading and processing polygons...", 0.3f);
                var result = MapBuildPipeline.Build(File.ReadAllText(countriesPath), File.ReadAllText(placesPath), new MapBuildOptions());

                EditorUtility.DisplayProgressBar("Natural Earth import", "Validating against countries.json...", 0.7f);
                var simulation = new CountryCatalog(CountryDataParser.Parse(File.ReadAllText(Path.Combine(root, SimulationData))));
                MapValidator.ValidateAgainstSimulation(result.Catalog, simulation, result.Report);

                var bytes = MapCatalogSerializer.Write(result.Catalog);
                File.WriteAllBytes(Path.Combine(root, CatalogOutput), bytes);

                foreach (var table in result.NameTables)
                {
                    var path = Path.Combine(root, LocalizationFolder, "map-names." + table.Key + ".json");
                    File.WriteAllText(path, MapNameTableWriter.ToJson(table.Key, table.Value), new UTF8Encoding(false));
                }

                var report = BuildReport(result, bytes.Length);
                File.WriteAllText(Path.Combine(root, ReportOutput), report);
                AssetDatabase.Refresh();

                if (result.Report.HasErrors)
                {
                    Debug.LogError("[Map import] Finished with errors. The catalog was written but must not ship.\n" + report);
                }
                else
                {
                    Debug.Log("[Map import] Finished: " + result.Catalog.Countries.Count + " territories, " + result.Report.WarningCount + " warning(s).\n" + report);
                }

                MapValidationWindow.Show(report);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        [MenuItem("Nation/Map/Validate Map Data")]
        public static void Validate()
        {
            var root = ProjectRoot();
            var catalogPath = Path.Combine(root, CatalogOutput);
            if (!File.Exists(catalogPath))
            {
                EditorUtility.DisplayDialog("Map validation", "No map catalog found. Run Nation > Map > Import Natural Earth first.", "OK");
                return;
            }

            var catalog = MapCatalogSerializer.Read(File.ReadAllBytes(catalogPath));
            var simulation = new CountryCatalog(CountryDataParser.Parse(File.ReadAllText(Path.Combine(root, SimulationData))));
            var report = new MapValidationReport();
            MapValidator.ValidateCatalog(catalog, report);
            MapValidator.ValidateAgainstSimulation(catalog, simulation, report);

            var text = "Countries: " + catalog.Countries.Count + "\n" + report.Summary();
            Debug.Log("[Map validation]\n" + text);
            MapValidationWindow.Show(text);
        }

        private static string BuildReport(MapBuildResult result, int catalogBytes)
        {
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

            report.Append("Catalog size: ").Append(catalogBytes).Append(" bytes\n\n");
            report.Append(result.Report.Summary());
            return report.ToString();
        }

        private static string ProjectRoot()
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        }
    }

    /// <summary>Writes a generated locale table in the same shape as the hand-written ones.</summary>
    public static class MapNameTableWriter
    {
        public static string ToJson(string locale, System.Collections.Generic.Dictionary<string, string> strings)
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

    /// <summary>Scrollable window showing the latest import or validation report.</summary>
    public sealed class MapValidationWindow : EditorWindow
    {
        private string _text = string.Empty;
        private Vector2 _scroll;

        public static void Show(string text)
        {
            var window = GetWindow<MapValidationWindow>("Map Validation");
            window._text = text;
            window.Repaint();
        }

        private void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.TextArea(_text, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }
    }
}
