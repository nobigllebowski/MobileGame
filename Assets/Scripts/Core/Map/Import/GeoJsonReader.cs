using System.Collections.Generic;
using Nation.Core.Data;

namespace Nation.Core.Map.Import
{
    /// <summary>One GeoJSON feature: its attribute table and its polygons as rings of [longitude, latitude].</summary>
    public sealed class GeoFeature
    {
        public JsonValue Properties { get; }
        /// <summary>Polygons; each polygon is a list of rings; ring 0 is the outer boundary.</summary>
        public List<List<List<double[]>>> Polygons { get; }
        public bool IsPoint { get; }
        public double PointLongitude { get; }
        public double PointLatitude { get; }

        public GeoFeature(JsonValue properties, List<List<List<double[]>>> polygons)
        {
            Properties = properties;
            Polygons = polygons;
        }

        public GeoFeature(JsonValue properties, double longitude, double latitude)
        {
            Properties = properties;
            Polygons = new List<List<List<double[]>>>();
            IsPoint = true;
            PointLongitude = longitude;
            PointLatitude = latitude;
        }

        public string Text(string key, string fallback = "") => Properties[key].AsString(fallback);
        public double Number(string key, double fallback = 0) => Properties[key].AsDouble(fallback);
    }

    /// <summary>Minimal GeoJSON FeatureCollection reader for Polygon, MultiPolygon and Point geometries.</summary>
    public static class GeoJsonReader
    {
        public static List<GeoFeature> Read(string geoJson)
        {
            var root = JsonParser.Parse(geoJson);
            var features = new List<GeoFeature>();
            foreach (var feature in root["features"].Items)
            {
                var geometry = feature["geometry"];
                var type = geometry["type"].AsString(string.Empty);
                var properties = feature["properties"];

                switch (type)
                {
                    case "Polygon":
                        features.Add(new GeoFeature(properties, new List<List<List<double[]>>> { ReadPolygon(geometry["coordinates"]) }));
                        break;
                    case "MultiPolygon":
                    {
                        var polygons = new List<List<List<double[]>>>();
                        foreach (var polygon in geometry["coordinates"].Items)
                        {
                            polygons.Add(ReadPolygon(polygon));
                        }

                        features.Add(new GeoFeature(properties, polygons));
                        break;
                    }
                    case "Point":
                    {
                        var coordinates = geometry["coordinates"];
                        features.Add(new GeoFeature(properties, coordinates[0].AsDouble(), coordinates[1].AsDouble()));
                        break;
                    }
                }
            }

            return features;
        }

        private static List<List<double[]>> ReadPolygon(JsonValue coordinates)
        {
            var rings = new List<List<double[]>>();
            foreach (var ring in coordinates.Items)
            {
                var points = new List<double[]>(ring.Count);
                foreach (var point in ring.Items)
                {
                    points.Add(new[] { point[0].AsDouble(), point[1].AsDouble() });
                }

                rings.Add(points);
            }

            return rings;
        }
    }
}
