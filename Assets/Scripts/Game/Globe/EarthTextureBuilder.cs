using System;
using Nation.Core.Map;
using Nation.Core.Map.Geometry;
using UnityEngine;

namespace Nation.Game.Globe
{
    /// <summary>
    /// Rasterizes the map catalog into an equirectangular texture for the globe, so the menu Earth is the same
    /// real geography as the world map with no external imagery: R = land mask, G = coast softness, B = city lights.
    /// Built once per session (about 60 ms at 1024x512) and cached.
    /// </summary>
    public static class EarthTextureBuilder
    {
        private static Texture2D _cached;

        public static Texture2D Build(MapCatalog catalog, int width = 1024, int height = 512)
        {
            if (_cached != null)
            {
                return _cached;
            }

            var land = new byte[width * height];
            var lights = new float[width * height];

            foreach (var country in catalog.Countries)
            {
                var lod = country.Lod(MapZoomBand.Mid);
                for (var r = 0; r < lod.RingCount; r++)
                {
                    FillRing(lod, r, land, width, height);
                }

                if (country.HasCapital && country.PopulationEstimate > 0)
                {
                    var intensity = (float)Math.Min(1.0, Math.Log10(Math.Max(country.PopulationEstimate, 1e5)) / 9.0);
                    StampLight(lights, width, height, country.CapitalLongitude, country.CapitalLatitude, intensity);
                }
            }

            var pixels = new Color32[width * height];
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var index = y * width + x;
                    var isLand = land[index];
                    var coast = CoastSoftness(land, width, height, x, y);
                    var light = Mathf.Clamp01(lights[index]);
                    pixels[index] = new Color32((byte)(isLand * 255), (byte)(coast * 255f), (byte)(light * 255f), 255);
                }
            }

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, true) { name = "EarthMask" };
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.filterMode = FilterMode.Bilinear;
            texture.SetPixels32(pixels);
            texture.Apply(true, true);
            _cached = texture;
            return texture;
        }

        private static void FillRing(MapLod lod, int ringIndex, byte[] land, int width, int height)
        {
            var start = lod.RingStarts[ringIndex];
            var count = lod.RingLengths[ringIndex];
            if (count < 3)
            {
                return;
            }

            var xs = new float[count];
            var ys = new float[count];
            var minY = float.MaxValue;
            var maxY = float.MinValue;
            for (var i = 0; i < count; i++)
            {
                var p = lod.Vertex(start + i);
                EqualEarthProjection.Unproject(p.X, p.Y, out var lon, out var lat);
                xs[i] = (float)((lon + 180.0) / 360.0 * width);
                ys[i] = (float)((90.0 - lat) / 180.0 * height);
                if (ys[i] < minY) minY = ys[i];
                if (ys[i] > maxY) maxY = ys[i];
            }

            var crossings = new float[count];
            var rowStart = Math.Max(0, (int)Math.Floor(minY));
            var rowEnd = Math.Min(height - 1, (int)Math.Ceiling(maxY));
            for (var row = rowStart; row <= rowEnd; row++)
            {
                var sampleY = row + 0.5f;
                var crossingCount = 0;
                for (int i = 0, j = count - 1; i < count; j = i++)
                {
                    var yi = ys[i];
                    var yj = ys[j];
                    if ((yi > sampleY) != (yj > sampleY))
                    {
                        var t = (sampleY - yi) / (yj - yi);
                        crossings[crossingCount++] = xs[i] + t * (xs[j] - xs[i]);
                    }
                }

                Array.Sort(crossings, 0, crossingCount);
                for (var c = 0; c + 1 < crossingCount; c += 2)
                {
                    var from = Math.Max(0, (int)Math.Round(crossings[c]));
                    var to = Math.Min(width - 1, (int)Math.Round(crossings[c + 1]));
                    for (var x = from; x <= to; x++)
                    {
                        land[row * width + x] = 1;
                    }
                }
            }
        }

        private static void StampLight(float[] lights, int width, int height, double lon, double lat, float intensity)
        {
            var cx = (int)((lon + 180.0) / 360.0 * width);
            var cy = (int)((90.0 - lat) / 180.0 * height);
            var radius = 2 + (int)(intensity * 4);
            for (var dy = -radius; dy <= radius; dy++)
            {
                for (var dx = -radius; dx <= radius; dx++)
                {
                    var x = (cx + dx + width) % width;
                    var y = cy + dy;
                    if (y < 0 || y >= height)
                    {
                        continue;
                    }

                    var falloff = 1f - Mathf.Sqrt(dx * dx + dy * dy) / (radius + 1f);
                    if (falloff <= 0)
                    {
                        continue;
                    }

                    var index = y * width + x;
                    lights[index] = Mathf.Max(lights[index], falloff * intensity);
                }
            }
        }

        private static float CoastSoftness(byte[] land, int width, int height, int x, int y)
        {
            var center = land[y * width + x];
            var different = 0;
            for (var dy = -1; dy <= 1; dy++)
            {
                var yy = y + dy;
                if (yy < 0 || yy >= height) continue;
                for (var dx = -1; dx <= 1; dx++)
                {
                    var xx = (x + dx + width) % width;
                    if (land[yy * width + xx] != center) different++;
                }
            }

            return different / 8f;
        }
    }
}
