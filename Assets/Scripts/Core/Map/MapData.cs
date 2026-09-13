using System.Collections.Generic;
using Nation.Core.Data;

namespace Nation.Core.Map
{
    /// <summary>A geographic point in degrees.</summary>
    public readonly struct GeoPoint
    {
        public double Latitude { get; }
        public double Longitude { get; }

        public GeoPoint(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
    }

    /// <summary>Equirectangular projection into the unit square (x right, y down), used by the placeholder map.</summary>
    public static class MapProjection
    {
        public static double X(double longitude) => (longitude + 180.0) / 360.0;

        public static double Y(double latitude) => (90.0 - latitude) / 180.0;
    }

    /// <summary>A coarse land-mass silhouette. Vertices are in degrees, in drawing order.</summary>
    public sealed class LandmassOutline
    {
        public string Id { get; }
        public IReadOnlyList<GeoPoint> Points { get; }

        public LandmassOutline(string id, IReadOnlyList<GeoPoint> points)
        {
            Id = id;
            Points = points;
        }
    }

    public interface IMapDataProvider
    {
        IReadOnlyList<LandmassOutline> Landmasses { get; }
    }

    public sealed class MapData : IMapDataProvider
    {
        public IReadOnlyList<LandmassOutline> Landmasses { get; }

        public MapData(IReadOnlyList<LandmassOutline> landmasses)
        {
            Landmasses = landmasses;
        }

        /// <summary>Parses { "landmasses": [ { "id": "...", "points": [[lon, lat], ...] } ] }.</summary>
        public static MapData Parse(string json)
        {
            var root = JsonParser.Parse(json);
            var landmasses = new List<LandmassOutline>();
            foreach (var node in root["landmasses"].Items)
            {
                var points = new List<GeoPoint>();
                foreach (var pair in node["points"].Items)
                {
                    points.Add(new GeoPoint(pair[1].AsDouble(), pair[0].AsDouble()));
                }

                landmasses.Add(new LandmassOutline(node["id"].AsString(string.Empty), points));
            }

            return new MapData(landmasses);
        }
    }
}
