using System.Collections.Generic;

namespace Nation.Core.Map.Geometry
{
    /// <summary>
    /// Ear-clipping triangulation of a simple polygon (outer ring only). Holes are not carved: countries that
    /// sit inside another (Lesotho, San Marino) are drawn on top instead, which is cheaper and visually identical.
    /// Degenerate input falls back to a fan so the importer never produces an empty mesh.
    /// </summary>
    public static class EarClipTriangulator
    {
        public static List<int> Triangulate(IReadOnlyList<MapPoint> ring)
        {
            var count = ring.Count;
            var indices = new List<int>();
            if (count < 3)
            {
                return indices;
            }

            var order = new List<int>(count);
            var clockwise = PolygonSimplifier.SignedArea(ring) < 0;
            for (var i = 0; i < count; i++)
            {
                order.Add(clockwise ? count - 1 - i : i);
            }

            var guard = 0;
            while (order.Count > 3 && guard < count * count)
            {
                guard++;
                var earFound = false;
                for (var i = 0; i < order.Count; i++)
                {
                    var prev = order[(i + order.Count - 1) % order.Count];
                    var cur = order[i];
                    var next = order[(i + 1) % order.Count];
                    if (IsEar(ring, order, prev, cur, next))
                    {
                        indices.Add(prev);
                        indices.Add(cur);
                        indices.Add(next);
                        order.RemoveAt(i);
                        earFound = true;
                        break;
                    }
                }

                if (!earFound)
                {
                    // Self-intersecting or numerically degenerate remainder: fan the rest and stop.
                    for (var i = 1; i < order.Count - 1; i++)
                    {
                        indices.Add(order[0]);
                        indices.Add(order[i]);
                        indices.Add(order[i + 1]);
                    }

                    return indices;
                }
            }

            if (order.Count == 3)
            {
                indices.Add(order[0]);
                indices.Add(order[1]);
                indices.Add(order[2]);
            }

            return indices;
        }

        private static bool IsEar(IReadOnlyList<MapPoint> ring, List<int> order, int prev, int cur, int next)
        {
            var a = ring[prev];
            var b = ring[cur];
            var c = ring[next];
            if (Cross(a, b, c) <= 1e-12f)
            {
                return false;
            }

            for (var i = 0; i < order.Count; i++)
            {
                var index = order[i];
                if (index == prev || index == cur || index == next)
                {
                    continue;
                }

                if (PointInTriangle(ring[index], a, b, c))
                {
                    return false;
                }
            }

            return true;
        }

        private static float Cross(MapPoint a, MapPoint b, MapPoint c)
        {
            return (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
        }

        private static bool PointInTriangle(MapPoint p, MapPoint a, MapPoint b, MapPoint c)
        {
            var d1 = Cross(a, b, p);
            var d2 = Cross(b, c, p);
            var d3 = Cross(c, a, p);
            var hasNegative = d1 < 0 || d2 < 0 || d3 < 0;
            var hasPositive = d1 > 0 || d2 > 0 || d3 > 0;
            return !(hasNegative && hasPositive);
        }
    }

    /// <summary>Point-in-polygon and related queries used by selection.</summary>
    public static class PolygonQueries
    {
        public static bool Contains(IReadOnlyList<MapPoint> ring, float x, float y)
        {
            var inside = false;
            for (int i = 0, j = ring.Count - 1; i < ring.Count; j = i++)
            {
                var pi = ring[i];
                var pj = ring[j];
                if ((pi.Y > y) != (pj.Y > y) && x < (pj.X - pi.X) * (y - pi.Y) / (pj.Y - pi.Y) + pi.X)
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        public static MapBounds BoundsOf(IReadOnlyList<MapPoint> ring)
        {
            var bounds = MapBounds.Empty;
            for (var i = 0; i < ring.Count; i++)
            {
                bounds.Encapsulate(ring[i].X, ring[i].Y);
            }

            return bounds;
        }
    }
}
