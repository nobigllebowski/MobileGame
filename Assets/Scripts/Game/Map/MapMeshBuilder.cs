using System.Collections.Generic;
using Nation.Core.Map;
using Nation.Core.Map.Geometry;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nation.Game.Map
{
    /// <summary>Vertex range of one country inside a combined mesh, used for recoloring without rebuilding.</summary>
    public struct CountryVertexRange
    {
        public string Id;
        public int Start;
        public int Count;
        public int TriangleStart;
        public int TriangleCount;
    }

    /// <summary>
    /// Builds the combined meshes for one zoom band: a single land mesh (vertex-colored, one draw call) and a
    /// single border mesh (thin quads). Countries are appended by descending area with a tiny z step so small
    /// countries and enclaves always draw above their neighbors.
    /// </summary>
    public static class MapMeshBuilder
    {
        public const float DepthStep = -0.0004f;

        public sealed class LandResult
        {
            public Mesh Mesh;
            public Color32[] Colors;
            public List<CountryVertexRange> Ranges = new List<CountryVertexRange>();
            public Dictionary<string, int> RangeIndex = new Dictionary<string, int>();
        }

        public static LandResult BuildLand(IReadOnlyList<MapCountry> countriesByAreaDescending, MapZoomBand band, float scale)
        {
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var result = new LandResult();

            for (var c = 0; c < countriesByAreaDescending.Count; c++)
            {
                var country = countriesByAreaDescending[c];
                var lod = country.Lod(band);
                var z = c * DepthStep;
                var range = new CountryVertexRange { Id = country.Id, Start = vertices.Count, Count = lod.VertexCount, TriangleStart = triangles.Count, TriangleCount = lod.Triangles.Length };

                for (var v = 0; v < lod.VertexCount; v++)
                {
                    vertices.Add(new Vector3(lod.Vertices[v * 2] * scale, lod.Vertices[v * 2 + 1] * scale, z));
                }

                for (var t = 0; t < lod.Triangles.Length; t++)
                {
                    triangles.Add(range.Start + lod.Triangles[t]);
                }

                result.RangeIndex[country.Id] = result.Ranges.Count;
                result.Ranges.Add(range);
            }

            var mesh = new Mesh { name = "MapLand_" + band };
            mesh.indexFormat = vertices.Count > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16;
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            result.Colors = new Color32[vertices.Count];
            for (var i = 0; i < result.Colors.Length; i++)
            {
                result.Colors[i] = new Color32(42, 54, 80, 255);
            }

            mesh.SetColors(result.Colors);
            mesh.RecalculateBounds();
            mesh.UploadMeshData(false);
            result.Mesh = mesh;
            return result;
        }

        public static Mesh BuildBorders(IReadOnlyList<MapCountry> countries, MapZoomBand band, float scale, float halfWidthMapUnits, Color32 color, float z)
        {
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var colors = new List<Color32>();

            foreach (var country in countries)
            {
                var lod = country.Lod(band);
                for (var r = 0; r < lod.RingCount; r++)
                {
                    AppendRing(lod, r, scale, halfWidthMapUnits, color, z, vertices, triangles, colors);
                }
            }

            return Finish("MapBorders_" + band, vertices, triangles, colors);
        }

        /// <summary>Outline of a single country, used for the selection highlight and glow.</summary>
        public static Mesh BuildOutline(MapCountry country, MapZoomBand band, float scale, float halfWidthMapUnits, Color32 color, float z)
        {
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var colors = new List<Color32>();
            var lod = country.Lod(band);
            for (var r = 0; r < lod.RingCount; r++)
            {
                AppendRing(lod, r, scale, halfWidthMapUnits, color, z, vertices, triangles, colors);
            }

            return Finish("Outline_" + country.Id, vertices, triangles, colors);
        }

        /// <summary>Fill of a single country at a given depth, used for the selection tint.</summary>
        public static Mesh BuildFill(MapCountry country, MapZoomBand band, float scale, Color32 color, float z)
        {
            var lod = country.Lod(band);
            var vertices = new List<Vector3>(lod.VertexCount);
            var colors = new List<Color32>(lod.VertexCount);
            for (var v = 0; v < lod.VertexCount; v++)
            {
                vertices.Add(new Vector3(lod.Vertices[v * 2] * scale, lod.Vertices[v * 2 + 1] * scale, z));
                colors.Add(color);
            }

            var triangles = new List<int>(lod.Triangles);
            return Finish("Fill_" + country.Id, vertices, triangles, colors);
        }

        /// <summary>Graticule (meridians and parallels every 15 degrees) projected like the countries.</summary>
        public static Mesh BuildGraticule(float scale, float halfWidthMapUnits, Color32 color, float z)
        {
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var colors = new List<Color32>();

            for (var lon = -180; lon <= 180; lon += 15)
            {
                var points = new List<MapPoint>();
                for (var lat = -88; lat <= 88; lat += 4)
                {
                    points.Add(EqualEarthProjection.Project(lon, lat));
                }

                AppendPolyline(points, scale, halfWidthMapUnits, color, z, vertices, triangles, colors);
            }

            for (var lat = -75; lat <= 75; lat += 15)
            {
                var points = new List<MapPoint>();
                for (var lon = -180; lon <= 180; lon += 4)
                {
                    points.Add(EqualEarthProjection.Project(lon, lat));
                }

                AppendPolyline(points, scale, halfWidthMapUnits, color, z, vertices, triangles, colors);
            }

            return Finish("Graticule", vertices, triangles, colors);
        }

        public static Mesh BuildQuad(float width, float height, float z, string meshName)
        {
            var mesh = new Mesh { name = meshName };
            var hw = width * 0.5f;
            var hh = height * 0.5f;
            mesh.SetVertices(new List<Vector3> { new Vector3(-hw, -hh, z), new Vector3(hw, -hh, z), new Vector3(hw, hh, z), new Vector3(-hw, hh, z) });
            mesh.SetUVs(0, new List<Vector2> { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) });
            mesh.SetTriangles(new List<int> { 0, 1, 2, 0, 2, 3 }, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void AppendRing(MapLod lod, int ringIndex, float scale, float halfWidth, Color32 color, float z, List<Vector3> vertices, List<int> triangles, List<Color32> colors)
        {
            var start = lod.RingStarts[ringIndex];
            var length = lod.RingLengths[ringIndex];
            var points = new List<MapPoint>(length + 1);
            for (var i = 0; i < length; i++)
            {
                points.Add(lod.Vertex(start + i));
            }

            points.Add(lod.Vertex(start));
            AppendPolyline(points, scale, halfWidth, color, z, vertices, triangles, colors);
        }

        private static void AppendPolyline(List<MapPoint> points, float scale, float halfWidth, Color32 color, float z, List<Vector3> vertices, List<int> triangles, List<Color32> colors)
        {
            for (var i = 0; i < points.Count - 1; i++)
            {
                var a = points[i];
                var b = points[i + 1];
                var dx = b.X - a.X;
                var dy = b.Y - a.Y;
                var length = Mathf.Sqrt(dx * dx + dy * dy);
                if (length < 1e-7f)
                {
                    continue;
                }

                var nx = -dy / length * halfWidth;
                var ny = dx / length * halfWidth;
                var baseIndex = vertices.Count;
                vertices.Add(new Vector3((a.X + nx) * scale, (a.Y + ny) * scale, z));
                vertices.Add(new Vector3((b.X + nx) * scale, (b.Y + ny) * scale, z));
                vertices.Add(new Vector3((b.X - nx) * scale, (b.Y - ny) * scale, z));
                vertices.Add(new Vector3((a.X - nx) * scale, (a.Y - ny) * scale, z));
                colors.Add(color);
                colors.Add(color);
                colors.Add(color);
                colors.Add(color);
                triangles.Add(baseIndex);
                triangles.Add(baseIndex + 1);
                triangles.Add(baseIndex + 2);
                triangles.Add(baseIndex);
                triangles.Add(baseIndex + 2);
                triangles.Add(baseIndex + 3);
            }
        }

        private static Mesh Finish(string meshName, List<Vector3> vertices, List<int> triangles, List<Color32> colors)
        {
            var mesh = new Mesh { name = meshName };
            mesh.indexFormat = vertices.Count > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16;
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetColors(colors);
            mesh.RecalculateBounds();
            mesh.UploadMeshData(false);
            return mesh;
        }
    }
}
