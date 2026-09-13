using System.Collections.Generic;

namespace Nation.Core.Map.Geometry
{
    /// <summary>
    /// Ramer-Douglas-Peucker simplification of closed rings in projected space. Produces the three zoom-band
    /// levels of detail from one source dataset so borders stay consistent between levels.
    /// </summary>
    public static class PolygonSimplifier
    {
        /// <summary>Simplifies a closed ring (first point not repeated). Returns at least a triangle when possible.</summary>
        public static List<MapPoint> Simplify(IReadOnlyList<MapPoint> ring, float tolerance)
        {
            if (ring.Count <= 4 || tolerance <= 0)
            {
                return new List<MapPoint>(ring);
            }

            // Split the ring at its two most distant points so the recursion has a stable anchor pair.
            var far = FarthestIndex(ring, 0);
            var keep = new bool[ring.Count];
            keep[0] = true;
            keep[far] = true;
            SimplifyRange(ring, 0, far, tolerance, keep);
            SimplifyRangeWrapped(ring, far, tolerance, keep);

            var result = new List<MapPoint>();
            for (var i = 0; i < ring.Count; i++)
            {
                if (keep[i])
                {
                    result.Add(ring[i]);
                }
            }

            if (result.Count < 3)
            {
                return new List<MapPoint>(ring);
            }

            return result;
        }

        private static int FarthestIndex(IReadOnlyList<MapPoint> ring, int from)
        {
            var best = from;
            var bestDistance = -1f;
            for (var i = 0; i < ring.Count; i++)
            {
                var dx = ring[i].X - ring[from].X;
                var dy = ring[i].Y - ring[from].Y;
                var d = dx * dx + dy * dy;
                if (d > bestDistance)
                {
                    bestDistance = d;
                    best = i;
                }
            }

            return best;
        }

        private static void SimplifyRange(IReadOnlyList<MapPoint> points, int first, int last, float tolerance, bool[] keep)
        {
            var stack = new Stack<KeyValuePair<int, int>>();
            stack.Push(new KeyValuePair<int, int>(first, last));
            var toleranceSquared = tolerance * tolerance;

            while (stack.Count > 0)
            {
                var range = stack.Pop();
                var a = range.Key;
                var b = range.Value;
                if (b - a < 2)
                {
                    continue;
                }

                var maxDistance = -1f;
                var index = -1;
                for (var i = a + 1; i < b; i++)
                {
                    var d = DistanceSquaredToSegment(points[i], points[a], points[b]);
                    if (d > maxDistance)
                    {
                        maxDistance = d;
                        index = i;
                    }
                }

                if (index >= 0 && maxDistance > toleranceSquared)
                {
                    keep[index] = true;
                    stack.Push(new KeyValuePair<int, int>(a, index));
                    stack.Push(new KeyValuePair<int, int>(index, b));
                }
            }
        }

        /// <summary>Handles the second half of the ring, from `far` around the end back to index 0.</summary>
        private static void SimplifyRangeWrapped(IReadOnlyList<MapPoint> ring, int far, float tolerance, bool[] keep)
        {
            var count = ring.Count - far + 1;
            var wrapped = new List<MapPoint>(count);
            for (var i = far; i < ring.Count; i++)
            {
                wrapped.Add(ring[i]);
            }

            wrapped.Add(ring[0]);

            var localKeep = new bool[wrapped.Count];
            localKeep[0] = true;
            localKeep[wrapped.Count - 1] = true;
            SimplifyRange(wrapped, 0, wrapped.Count - 1, tolerance, localKeep);

            for (var i = 1; i < wrapped.Count - 1; i++)
            {
                if (localKeep[i])
                {
                    keep[far + i] = true;
                }
            }
        }

        public static float DistanceSquaredToSegment(MapPoint p, MapPoint a, MapPoint b)
        {
            var abx = b.X - a.X;
            var aby = b.Y - a.Y;
            var apx = p.X - a.X;
            var apy = p.Y - a.Y;
            var lengthSquared = abx * abx + aby * aby;
            var t = lengthSquared <= 0 ? 0f : (apx * abx + apy * aby) / lengthSquared;
            if (t < 0) t = 0;
            if (t > 1) t = 1;
            var cx = a.X + abx * t - p.X;
            var cy = a.Y + aby * t - p.Y;
            return cx * cx + cy * cy;
        }

        /// <summary>Signed area of a ring; positive when counter-clockwise.</summary>
        public static double SignedArea(IReadOnlyList<MapPoint> ring)
        {
            double area = 0;
            for (int i = 0, j = ring.Count - 1; i < ring.Count; j = i++)
            {
                area += (double)ring[j].X * ring[i].Y - (double)ring[i].X * ring[j].Y;
            }

            return area * 0.5;
        }

        /// <summary>Removes consecutive duplicate points and a repeated closing point.</summary>
        public static List<MapPoint> Clean(IReadOnlyList<MapPoint> ring)
        {
            var result = new List<MapPoint>(ring.Count);
            for (var i = 0; i < ring.Count; i++)
            {
                var p = ring[i];
                if (result.Count > 0)
                {
                    var last = result[result.Count - 1];
                    if (System.Math.Abs(last.X - p.X) < 1e-7f && System.Math.Abs(last.Y - p.Y) < 1e-7f)
                    {
                        continue;
                    }
                }

                result.Add(p);
            }

            if (result.Count > 1)
            {
                var first = result[0];
                var last = result[result.Count - 1];
                if (System.Math.Abs(last.X - first.X) < 1e-7f && System.Math.Abs(last.Y - first.Y) < 1e-7f)
                {
                    result.RemoveAt(result.Count - 1);
                }
            }

            return result;
        }
    }
}
