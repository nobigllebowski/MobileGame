using System;

namespace Nation.Core.Map.Geometry
{
    /// <summary>
    /// Equal Earth projection (Šavrič, Patterson, Jenny 2018): an equal-area pseudocylindrical projection with a
    /// closed-form forward transform. Chosen for a modern atlas look without Mercator's polar distortion.
    /// Output is in "map units": longitude 180 maps to x ≈ 2.71, latitude 90 to y ≈ 1.32.
    /// </summary>
    public static class EqualEarthProjection
    {
        private const double A1 = 1.340264;
        private const double A2 = -0.081106;
        private const double A3 = 0.000893;
        private const double A4 = 0.003796;
        private static readonly double M = Math.Sqrt(3.0) / 2.0;

        public const double MaxX = 2.7066;
        public const double MaxY = 1.3173;

        public static void Project(double longitude, double latitude, out double x, out double y)
        {
            var lambda = longitude * Math.PI / 180.0;
            var phi = latitude * Math.PI / 180.0;
            var theta = Math.Asin(M * Math.Sin(phi));
            var t2 = theta * theta;
            var t6 = t2 * t2 * t2;
            x = lambda * Math.Cos(theta) / (M * (A1 + 3 * A2 * t2 + t6 * (7 * A3 + 9 * A4 * t2)));
            y = theta * (A1 + A2 * t2 + t6 * (A3 + A4 * t2));
        }

        public static MapPoint Project(double longitude, double latitude)
        {
            Project(longitude, latitude, out var x, out var y);
            return new MapPoint((float)x, (float)y);
        }

        /// <summary>Inverse by Newton iteration on theta; used for debugging and camera bounds, not per frame.</summary>
        public static void Unproject(double x, double y, out double longitude, out double latitude)
        {
            var theta = y;
            for (var i = 0; i < 12; i++)
            {
                var t2 = theta * theta;
                var t6 = t2 * t2 * t2;
                var f = theta * (A1 + A2 * t2 + t6 * (A3 + A4 * t2)) - y;
                var df = A1 + 3 * A2 * t2 + t6 * (7 * A3 + 9 * A4 * t2);
                var delta = f / df;
                theta -= delta;
                if (Math.Abs(delta) < 1e-12)
                {
                    break;
                }
            }

            var tt2 = theta * theta;
            var tt6 = tt2 * tt2 * tt2;
            var lambda = M * x * (A1 + 3 * A2 * tt2 + tt6 * (7 * A3 + 9 * A4 * tt2)) / Math.Cos(theta);
            var phi = Math.Asin(Math.Sin(theta) / M);
            longitude = lambda * 180.0 / Math.PI;
            latitude = phi * 180.0 / Math.PI;
        }
    }

    /// <summary>A projected point in map units.</summary>
    public readonly struct MapPoint
    {
        public float X { get; }
        public float Y { get; }

        public MapPoint(float x, float y)
        {
            X = x;
            Y = y;
        }
    }

    /// <summary>Axis-aligned bounds in map units.</summary>
    public struct MapBounds
    {
        public float MinX;
        public float MinY;
        public float MaxX;
        public float MaxY;

        public static MapBounds Empty => new MapBounds { MinX = float.MaxValue, MinY = float.MaxValue, MaxX = float.MinValue, MaxY = float.MinValue };

        public float Width => MaxX - MinX;
        public float Height => MaxY - MinY;
        public float CenterX => (MinX + MaxX) * 0.5f;
        public float CenterY => (MinY + MaxY) * 0.5f;
        public bool IsValid => MaxX >= MinX && MaxY >= MinY;

        public void Encapsulate(float x, float y)
        {
            if (x < MinX) MinX = x;
            if (y < MinY) MinY = y;
            if (x > MaxX) MaxX = x;
            if (y > MaxY) MaxY = y;
        }

        public void Encapsulate(MapBounds other)
        {
            if (!other.IsValid) return;
            Encapsulate(other.MinX, other.MinY);
            Encapsulate(other.MaxX, other.MaxY);
        }

        public bool Contains(float x, float y)
        {
            return x >= MinX && x <= MaxX && y >= MinY && y <= MaxY;
        }

        public bool Intersects(MapBounds other)
        {
            return other.MaxX >= MinX && other.MinX <= MaxX && other.MaxY >= MinY && other.MinY <= MaxY;
        }
    }
}
