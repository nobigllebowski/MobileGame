using System;
using System.Collections.Generic;
using Nation.Core.Map.Geometry;

namespace Nation.Core.Map.Import
{
    public sealed class MapBuildOptions
    {
        /// <summary>Douglas-Peucker tolerance in map units per zoom band (far, mid, near).</summary>
        public float[] Tolerances { get; set; } = { 0.012f, 0.004f, 0.0007f };

        /// <summary>Rings smaller than this (map units squared) are dropped per band so islets vanish when zoomed out.</summary>
        public float[] MinRingArea { get; set; } = { 0.002f, 0.0003f, 0.00002f };

        public bool IncludeAntarctica { get; set; } = true;
        public string SourceDescription { get; set; } = "Natural Earth admin-0 countries and populated places, 1:50m";
    }

    /// <summary>Everything the pipeline produces: the catalog, per-locale name tables and the validation report.</summary>
    public sealed class MapBuildResult
    {
        public MapCatalog Catalog { get; set; }
        public Dictionary<string, Dictionary<string, string>> NameTables { get; set; } = new Dictionary<string, Dictionary<string, string>>();
        public MapValidationReport Report { get; set; } = new MapValidationReport();
        public int SourceVertices { get; set; }
    }

    /// <summary>
    /// SOURCE GEO DATA → IMPORT → VALIDATE IDS → PROCESS POLYGONS → CATALOG.
    /// Deterministic: countries are sorted by id and every step is pure arithmetic on the input.
    /// Engine-free so the Editor menu, EditMode tests and the command-line harness run the same code.
    /// </summary>
    public static class MapBuildPipeline
    {
        public static MapBuildResult Build(string countriesGeoJson, string placesGeoJson, MapBuildOptions options)
        {
            options = options ?? new MapBuildOptions();
            var result = new MapBuildResult();
            var report = result.Report;

            var countryFeatures = GeoJsonReader.Read(countriesGeoJson);
            var placeFeatures = placesGeoJson == null ? new List<GeoFeature>() : GeoJsonReader.Read(placesGeoJson);
            var capitals = IndexCapitals(placeFeatures);
            var sovereignByCode = IndexSovereigns(countryFeatures);

            foreach (var pair in NaturalEarthMapping.NameLocales)
            {
                result.NameTables[pair.Key] = new Dictionary<string, string>(StringComparer.Ordinal);
            }

            var catalog = new MapCatalog { Source = options.SourceDescription };
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var worldBounds = MapBounds.Empty;

            foreach (var feature in countryFeatures)
            {
                var id = NaturalEarthMapping.ResolveId(feature, seen, out var isIso);
                var name = feature.Text("NAME_EN", feature.Text("NAME"));
                if (string.IsNullOrEmpty(id))
                {
                    report.Add(MapIssueSeverity.Error, MapValidationReport.MissingIso, null, "Feature '" + name + "' has no usable code.");
                    continue;
                }

                if (!seen.Add(id))
                {
                    report.Add(MapIssueSeverity.Error, MapValidationReport.DuplicateId, id, "Feature '" + name + "' duplicates an existing id.");
                    continue;
                }

                var type = NaturalEarthMapping.ResolveType(feature);
                if (id == "ATA" && !options.IncludeAntarctica)
                {
                    continue;
                }

                var country = new MapCountry
                {
                    Id = id,
                    IsIsoCode = isIso,
                    Type = type,
                    Continent = feature.Text("CONTINENT"),
                    Selectable = type != MapTerritoryType.Indeterminate,
                    LabelRank = (int)feature.Number("LABELRANK", 5),
                    MinLabelZoom = (float)feature.Number("MIN_LABEL", 4),
                    PopulationEstimate = feature.Number("POP_EST"),
                    GdpEstimate = feature.Number("GDP_MD") * 1e6
                };

                var sovereignCode = feature.Text("SOV_A3");
                country.SovereignId = sovereignByCode.TryGetValue(sovereignCode, out var sovereignId) ? sovereignId : id;

                var label = EqualEarthProjection.Project(feature.Number("LABEL_X"), feature.Number("LABEL_Y"));
                country.LabelX = label.X;
                country.LabelY = label.Y;

                var vertices = BuildGeometry(feature, options, country, report);
                result.SourceVertices += vertices;
                if (country.Lods[2].VertexCount == 0)
                {
                    report.Add(MapIssueSeverity.Error, MapValidationReport.EmptyGeometry, id, "Feature '" + name + "' produced no usable polygons.");
                    continue;
                }

                if (feature.Properties.Has("LABEL_X") == false || (Math.Abs(country.LabelX) < 1e-6f && Math.Abs(country.LabelY) < 1e-6f))
                {
                    country.LabelX = country.Bounds.CenterX;
                    country.LabelY = country.Bounds.CenterY;
                }

                AssignCapital(country, capitals, result.NameTables, report);
                CollectNames(feature, country, result.NameTables);

                worldBounds.Encapsulate(country.Bounds);
                catalog.Countries.Add(country);
            }

            catalog.Countries.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id));
            catalog.Bounds = worldBounds;
            result.Catalog = catalog;

            MapValidator.ValidateCatalog(catalog, report);
            return result;
        }

        private static Dictionary<string, string> IndexSovereigns(List<GeoFeature> features)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var feature in features)
            {
                if (NaturalEarthMapping.ResolveType(feature) != MapTerritoryType.SovereignCountry)
                {
                    continue;
                }

                var code = feature.Text("SOV_A3");
                var id = NaturalEarthMapping.ResolveId(feature, out _);
                if (!string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(id) && !map.ContainsKey(code))
                {
                    map[code] = id;
                }
            }

            return map;
        }

        private static Dictionary<string, List<GeoFeature>> IndexCapitals(List<GeoFeature> places)
        {
            var map = new Dictionary<string, List<GeoFeature>>(StringComparer.OrdinalIgnoreCase);
            foreach (var place in places)
            {
                if (!place.IsPoint || !NaturalEarthMapping.IsAdmin0Capital(place))
                {
                    continue;
                }

                var code = place.Text("ADM0_A3");
                if (string.IsNullOrEmpty(code))
                {
                    continue;
                }

                if (!map.TryGetValue(code, out var list))
                {
                    list = new List<GeoFeature>();
                    map[code] = list;
                }

                list.Add(place);
            }

            return map;
        }

        private static int BuildGeometry(GeoFeature feature, MapBuildOptions options, MapCountry country, MapValidationReport report)
        {
            var sourceVertices = 0;
            var projectedRings = new List<List<MapPoint>>();
            foreach (var polygon in feature.Polygons)
            {
                if (polygon.Count == 0)
                {
                    continue;
                }

                var outer = polygon[0];
                sourceVertices += outer.Count;
                var ring = new List<MapPoint>(outer.Count);
                foreach (var point in outer)
                {
                    ring.Add(EqualEarthProjection.Project(point[0], point[1]));
                }

                ring = PolygonSimplifier.Clean(ring);
                if (ring.Count < 3)
                {
                    report.Add(MapIssueSeverity.Warning, MapValidationReport.InvalidPolygon, country.Id, "Skipped a degenerate source ring.");
                    continue;
                }

                if (PolygonSimplifier.SignedArea(ring) < 0)
                {
                    ring.Reverse();
                }

                projectedRings.Add(ring);
            }

            var bounds = MapBounds.Empty;
            double area = 0;
            for (var band = 0; band < 3; band++)
            {
                var lod = BuildLod(projectedRings, options.Tolerances[band], options.MinRingArea[band]);
                country.Lods[band] = lod;
            }

            // Guarantee the largest ring survives at every band even if it is below the area threshold.
            for (var band = 0; band < 3; band++)
            {
                if (country.Lods[band].VertexCount == 0 && projectedRings.Count > 0)
                {
                    country.Lods[band] = BuildLod(new List<List<MapPoint>> { LargestRing(projectedRings) }, options.Tolerances[band], 0f);
                }
            }

            var near = country.Lods[2];
            for (var r = 0; r < near.RingCount; r++)
            {
                var ring = near.Ring(r);
                bounds.Encapsulate(PolygonQueries.BoundsOf(ring));
                area += Math.Abs(PolygonSimplifier.SignedArea(ring));
            }

            country.Bounds = bounds;
            country.Area = area;
            return sourceVertices;
        }

        private static List<MapPoint> LargestRing(List<List<MapPoint>> rings)
        {
            List<MapPoint> best = null;
            double bestArea = -1;
            foreach (var ring in rings)
            {
                var area = Math.Abs(PolygonSimplifier.SignedArea(ring));
                if (area > bestArea)
                {
                    bestArea = area;
                    best = ring;
                }
            }

            return best;
        }

        private static MapLod BuildLod(List<List<MapPoint>> rings, float tolerance, float minArea)
        {
            var vertices = new List<float>();
            var starts = new List<int>();
            var lengths = new List<int>();
            var triangles = new List<int>();

            foreach (var source in rings)
            {
                var ring = PolygonSimplifier.Simplify(source, tolerance);
                ring = PolygonSimplifier.Clean(ring);
                if (ring.Count < 3)
                {
                    continue;
                }

                var area = Math.Abs(PolygonSimplifier.SignedArea(ring));
                if (area < minArea)
                {
                    continue;
                }

                if (PolygonSimplifier.SignedArea(ring) < 0)
                {
                    ring.Reverse();
                }

                var start = vertices.Count / 2;
                var local = EarClipTriangulator.Triangulate(ring);
                if (local.Count < 3)
                {
                    continue;
                }

                starts.Add(start);
                lengths.Add(ring.Count);
                foreach (var point in ring)
                {
                    vertices.Add(point.X);
                    vertices.Add(point.Y);
                }

                foreach (var index in local)
                {
                    triangles.Add(start + index);
                }
            }

            return new MapLod
            {
                Vertices = vertices.ToArray(),
                RingStarts = starts.ToArray(),
                RingLengths = lengths.ToArray(),
                Triangles = triangles.ToArray()
            };
        }

        private static void AssignCapital(MapCountry country, Dictionary<string, List<GeoFeature>> capitals, Dictionary<string, Dictionary<string, string>> names, MapValidationReport report)
        {
            if (!capitals.TryGetValue(country.Id, out var candidates) || candidates.Count == 0)
            {
                return;
            }

            GeoFeature chosen = null;
            if (NaturalEarthMapping.CapitalOverrides.TryGetValue(country.Id, out var preferred))
            {
                foreach (var candidate in candidates)
                {
                    if (string.Equals(candidate.Text("NAME"), preferred, StringComparison.OrdinalIgnoreCase))
                    {
                        chosen = candidate;
                        break;
                    }
                }
            }

            if (chosen == null)
            {
                double bestPopulation = -1;
                foreach (var candidate in candidates)
                {
                    var flagged = candidate.Number("ADM0CAP") >= 1 ? 1e12 : 0;
                    var population = candidate.Number("POP_MAX") + flagged;
                    if (population > bestPopulation)
                    {
                        bestPopulation = population;
                        chosen = candidate;
                    }
                }
            }

            if (chosen == null)
            {
                return;
            }

            country.HasCapital = true;
            country.CapitalLongitude = chosen.PointLongitude;
            country.CapitalLatitude = chosen.PointLatitude;
            var projected = EqualEarthProjection.Project(chosen.PointLongitude, chosen.PointLatitude);
            country.CapitalX = projected.X;
            country.CapitalY = projected.Y;

            var english = chosen.Text("NAME_EN", chosen.Text("NAME"));
            foreach (var pair in NaturalEarthMapping.NameLocales)
            {
                var value = chosen.Text(pair.Value);
                if (string.IsNullOrEmpty(value) || value == NaturalEarthMapping.MissingValue)
                {
                    value = english;
                }

                if (!string.IsNullOrEmpty(value))
                {
                    names[pair.Key][country.CapitalKey] = value;
                }
            }
        }

        private static void CollectNames(GeoFeature feature, MapCountry country, Dictionary<string, Dictionary<string, string>> names)
        {
            var english = feature.Text("NAME_EN", feature.Text("NAME"));
            foreach (var pair in NaturalEarthMapping.NameLocales)
            {
                var value = feature.Text(pair.Value);
                if (string.IsNullOrEmpty(value) || value == NaturalEarthMapping.MissingValue)
                {
                    value = english;
                }

                if (!string.IsNullOrEmpty(value))
                {
                    names[pair.Key][country.NameKey] = value.ToUpperInvariant();
                }
            }
        }
    }
}
