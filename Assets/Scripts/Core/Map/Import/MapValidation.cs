using System.Collections.Generic;
using System.Text;
using Nation.Core.Countries;
using Nation.Core.Map.Geometry;

namespace Nation.Core.Map.Import
{
    public enum MapIssueSeverity
    {
        Info,
        Warning,
        Error
    }

    public readonly struct MapIssue
    {
        public MapIssueSeverity Severity { get; }
        public string Code { get; }
        public string CountryId { get; }
        public string Message { get; }

        public MapIssue(MapIssueSeverity severity, string code, string countryId, string message)
        {
            Severity = severity;
            Code = code;
            CountryId = countryId;
            Message = message;
        }

        public override string ToString() => Severity + " " + Code + (string.IsNullOrEmpty(CountryId) ? "" : " [" + CountryId + "]") + ": " + Message;
    }

    /// <summary>Collects import and cross-check findings. Errors mean the catalog must not ship.</summary>
    public sealed class MapValidationReport
    {
        public const string MissingIso = "missing-iso3";
        public const string NonIsoCode = "non-iso-code";
        public const string DuplicateId = "duplicate-id";
        public const string InvalidPolygon = "invalid-polygon";
        public const string EmptyGeometry = "empty-geometry";
        public const string MissingCapital = "missing-capital";
        public const string CapitalOutside = "capital-outside";
        public const string SimulationMissing = "simulation-country-missing";
        public const string DataMismatch = "country-data-mismatch";

        public List<MapIssue> Issues { get; } = new List<MapIssue>();

        public int ErrorCount { get; private set; }
        public int WarningCount { get; private set; }
        public bool HasErrors => ErrorCount > 0;

        public void Add(MapIssueSeverity severity, string code, string countryId, string message)
        {
            Issues.Add(new MapIssue(severity, code, countryId, message));
            if (severity == MapIssueSeverity.Error) ErrorCount++;
            if (severity == MapIssueSeverity.Warning) WarningCount++;
        }

        public int Count(string code)
        {
            var count = 0;
            foreach (var issue in Issues)
            {
                if (issue.Code == code) count++;
            }

            return count;
        }

        public string Summary()
        {
            var builder = new StringBuilder();
            builder.Append("Map validation: ").Append(ErrorCount).Append(" error(s), ").Append(WarningCount).Append(" warning(s)\n");
            foreach (var issue in Issues)
            {
                builder.Append("  ").Append(issue).Append('\n');
            }

            return builder.ToString();
        }
    }

    /// <summary>Checks a built catalog on its own and against the simulation country data.</summary>
    public static class MapValidator
    {
        public static void ValidateCatalog(MapCatalog catalog, MapValidationReport report)
        {
            var seen = new HashSet<string>();
            foreach (var country in catalog.Countries)
            {
                if (string.IsNullOrEmpty(country.Id) || country.Id.Length != 3)
                {
                    report.Add(MapIssueSeverity.Error, MapValidationReport.MissingIso, country.Id, "Country has no three-letter id.");
                    continue;
                }

                if (!seen.Add(country.Id))
                {
                    report.Add(MapIssueSeverity.Error, MapValidationReport.DuplicateId, country.Id, "Duplicate country id in catalog.");
                }

                if (!country.IsIsoCode)
                {
                    report.Add(MapIssueSeverity.Info, MapValidationReport.NonIsoCode, country.Id, "Territory uses a Natural Earth code instead of an ISO 3166-1 code.");
                }

                for (var band = 0; band < country.Lods.Length; band++)
                {
                    var lod = country.Lods[band];
                    if (lod.VertexCount < 3 || lod.Triangles.Length < 3)
                    {
                        report.Add(MapIssueSeverity.Error, MapValidationReport.EmptyGeometry, country.Id, "No geometry at zoom band " + (MapZoomBand)band + ".");
                    }

                    for (var r = 0; r < lod.RingCount; r++)
                    {
                        if (lod.RingLengths[r] < 3)
                        {
                            report.Add(MapIssueSeverity.Error, MapValidationReport.InvalidPolygon, country.Id, "Ring " + r + " at band " + (MapZoomBand)band + " has fewer than three points.");
                        }
                    }
                }

                if (country.Type == MapTerritoryType.SovereignCountry || country.Type == MapTerritoryType.Country)
                {
                    if (!country.HasCapital)
                    {
                        report.Add(MapIssueSeverity.Warning, MapValidationReport.MissingCapital, country.Id, "No admin-0 capital found in populated places.");
                    }
                    else if (!ContainsPoint(country, country.CapitalX, country.CapitalY))
                    {
                        report.Add(MapIssueSeverity.Warning, MapValidationReport.CapitalOutside, country.Id, "Capital point lies outside the country's polygons.");
                    }
                }
            }
        }

        public static void ValidateAgainstSimulation(MapCatalog catalog, ICountryDataProvider simulation, MapValidationReport report)
        {
            foreach (var definition in simulation.All)
            {
                if (!catalog.TryGet(definition.Id, out var mapCountry))
                {
                    report.Add(MapIssueSeverity.Error, MapValidationReport.SimulationMissing, definition.Id, "Simulated country has no map geometry.");
                    continue;
                }

                if (!mapCountry.Selectable)
                {
                    report.Add(MapIssueSeverity.Error, MapValidationReport.DataMismatch, definition.Id, "Simulated country is marked non-selectable on the map.");
                }

                var projected = EqualEarthProjection.Project(definition.CapitalLongitude, definition.CapitalLatitude);
                if (!ContainsPoint(mapCountry, projected.X, projected.Y))
                {
                    report.Add(MapIssueSeverity.Warning, MapValidationReport.DataMismatch, definition.Id, "countries.json capital coordinates fall outside the map polygons.");
                }

                if (mapCountry.PopulationEstimate > 0)
                {
                    var ratio = definition.Population / mapCountry.PopulationEstimate;
                    if (ratio < 0.5 || ratio > 2.0)
                    {
                        report.Add(MapIssueSeverity.Warning, MapValidationReport.DataMismatch, definition.Id, "countries.json population differs from the map estimate by more than 2x.");
                    }
                }
            }
        }

        /// <summary>Coastal capitals sit on the shoreline, so a point counts as inside when within ~40 km of an edge.</summary>
        public const float CoastTolerance = 0.006f;

        public static bool ContainsPoint(MapCountry country, float x, float y)
        {
            return ContainsPoint(country, x, y, CoastTolerance);
        }

        public static bool ContainsPoint(MapCountry country, float x, float y, float tolerance)
        {
            var lod = country.Lods[country.Lods.Length - 1];
            var toleranceSquared = tolerance * tolerance;
            for (var r = 0; r < lod.RingCount; r++)
            {
                var ring = lod.Ring(r);
                if (PolygonQueries.Contains(ring, x, y))
                {
                    return true;
                }

                if (tolerance <= 0)
                {
                    continue;
                }

                var point = new MapPoint(x, y);
                for (int i = 0, j = ring.Count - 1; i < ring.Count; j = i++)
                {
                    if (PolygonSimplifier.DistanceSquaredToSegment(point, ring[j], ring[i]) <= toleranceSquared)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
