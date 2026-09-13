using System.Collections.Generic;
using Nation.Core.Map.Geometry;
using NUnit.Framework;

namespace Nation.Tests
{
    public sealed class MapGeometryTests
    {
        [Test]
        public void Equal_Earth_Projects_Origin_And_Edges()
        {
            EqualEarthProjection.Project(0, 0, out var x, out var y);
            Assert.AreEqual(0.0, x);
            Assert.AreEqual(0.0, y);

            EqualEarthProjection.Project(180, 0, out var maxX, out _);
            Assert.IsTrue(maxX > 2.7 && maxX < 2.71);

            EqualEarthProjection.Project(0, 90, out _, out var maxY);
            Assert.IsTrue(maxY > 1.31 && maxY < 1.32);

            EqualEarthProjection.Project(13.4, 52.5, out var bx, out var by);
            Assert.IsTrue(bx > 0 && by > 0);
        }

        [Test]
        public void Equal_Earth_Round_Trips_Through_Inverse()
        {
            EqualEarthProjection.Project(-77.03, 38.9, out var x, out var y);
            EqualEarthProjection.Unproject(x, y, out var lon, out var lat);

            Assert.IsTrue(System.Math.Abs(lon + 77.03) < 1e-6);
            Assert.IsTrue(System.Math.Abs(lat - 38.9) < 1e-6);
        }

        [Test]
        public void Simplifier_Removes_Collinear_Points_And_Keeps_Corners()
        {
            var square = new List<MapPoint>
            {
                new MapPoint(0, 0), new MapPoint(0.5f, 0.001f), new MapPoint(1, 0),
                new MapPoint(1, 0.5f), new MapPoint(1, 1), new MapPoint(0.5f, 1), new MapPoint(0, 1), new MapPoint(0, 0.5f)
            };

            var simplified = PolygonSimplifier.Simplify(square, 0.01f);

            Assert.AreEqual(4, simplified.Count);
            Assert.AreEqual(square, PolygonSimplifier.Simplify(square, 0f));
        }

        [Test]
        public void Triangulator_Handles_Concave_Polygon()
        {
            var arrow = new List<MapPoint>
            {
                new MapPoint(0, 0), new MapPoint(2, 0), new MapPoint(2, 2), new MapPoint(1, 0.5f), new MapPoint(0, 2)
            };

            var triangles = EarClipTriangulator.Triangulate(arrow);

            Assert.AreEqual((arrow.Count - 2) * 3, triangles.Count);
            foreach (var index in triangles)
            {
                Assert.IsTrue(index >= 0 && index < arrow.Count);
            }
        }

        [Test]
        public void Triangulator_Accepts_Clockwise_Input()
        {
            var clockwise = new List<MapPoint> { new MapPoint(0, 0), new MapPoint(0, 1), new MapPoint(1, 1), new MapPoint(1, 0) };

            Assert.AreEqual(6, EarClipTriangulator.Triangulate(clockwise).Count);
        }

        [Test]
        public void Point_In_Polygon_Works_For_Inside_Outside_And_Concavity()
        {
            var arrow = new List<MapPoint>
            {
                new MapPoint(0, 0), new MapPoint(2, 0), new MapPoint(2, 2), new MapPoint(1, 0.5f), new MapPoint(0, 2)
            };

            Assert.IsTrue(PolygonQueries.Contains(arrow, 0.3f, 0.3f));
            Assert.IsFalse(PolygonQueries.Contains(arrow, 1f, 1.5f));
            Assert.IsFalse(PolygonQueries.Contains(arrow, 3f, 1f));
        }

        [Test]
        public void Bounds_Encapsulate_And_Intersect()
        {
            var a = MapBounds.Empty;
            a.Encapsulate(0, 0);
            a.Encapsulate(2, 1);
            var b = MapBounds.Empty;
            b.Encapsulate(1, 0.5f);
            b.Encapsulate(3, 3);

            Assert.IsTrue(a.IsValid);
            Assert.AreEqual(2f, a.Width);
            Assert.IsTrue(a.Intersects(b));
            Assert.IsTrue(a.Contains(1, 1));
            Assert.IsFalse(a.Contains(2.5f, 1));
        }
    }
}
