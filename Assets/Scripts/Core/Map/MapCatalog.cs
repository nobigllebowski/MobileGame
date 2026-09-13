using System;
using System.Collections.Generic;
using Nation.Core.Map.Geometry;

namespace Nation.Core.Map
{
    public enum MapZoomBand
    {
        Far = 0,
        Mid = 1,
        Near = 2
    }

    public enum MapTerritoryType
    {
        SovereignCountry,
        Country,
        Dependency,
        Disputed,
        Indeterminate
    }

    /// <summary>
    /// Processed geometry of one country at one zoom band: a flat vertex array (x, y pairs), the rings that
    /// make up its outline (as ranges into the vertex array) and a triangle index list for filling.
    /// </summary>
    public sealed class MapLod
    {
        public float[] Vertices { get; set; } = new float[0];
        public int[] RingStarts { get; set; } = new int[0];
        public int[] RingLengths { get; set; } = new int[0];
        public int[] Triangles { get; set; } = new int[0];

        public int VertexCount => Vertices.Length / 2;
        public int RingCount => RingStarts.Length;

        public MapPoint Vertex(int index) => new MapPoint(Vertices[index * 2], Vertices[index * 2 + 1]);

        public List<MapPoint> Ring(int ringIndex)
        {
            var start = RingStarts[ringIndex];
            var length = RingLengths[ringIndex];
            var ring = new List<MapPoint>(length);
            for (var i = 0; i < length; i++)
            {
                ring.Add(Vertex(start + i));
            }

            return ring;
        }
    }

    /// <summary>
    /// MAP identity of one territory. Deliberately separate from CountryDefinition (simulation): every territory
    /// on Earth has a MapCountry; only the simulated ones also have a CountryDefinition, joined by ISO3.
    /// Estimates come from the Natural Earth attributes and are display-only context for unsimulated countries.
    /// </summary>
    public sealed class MapCountry
    {
        public string Id { get; set; }
        public string SovereignId { get; set; }
        public MapTerritoryType Type { get; set; }
        public string Continent { get; set; }
        public bool IsIsoCode { get; set; } = true;
        public bool Selectable { get; set; } = true;

        public float LabelX { get; set; }
        public float LabelY { get; set; }
        public int LabelRank { get; set; }
        public float MinLabelZoom { get; set; }

        public bool HasCapital { get; set; }
        public float CapitalX { get; set; }
        public float CapitalY { get; set; }
        public double CapitalLatitude { get; set; }
        public double CapitalLongitude { get; set; }

        public double PopulationEstimate { get; set; }
        /// <summary>US dollars (Natural Earth GDP_MD converted from millions).</summary>
        public double GdpEstimate { get; set; }

        public MapBounds Bounds { get; set; }
        public double Area { get; set; }

        public MapLod[] Lods { get; set; } = new MapLod[3];

        public string NameKey => "country." + Id;
        public string CapitalKey => "capital." + Id;

        public MapLod Lod(MapZoomBand band) => Lods[(int)band];
    }

    /// <summary>The whole processed map. Built by the import pipeline, serialized to a compact binary file.</summary>
    public sealed class MapCatalog
    {
        public const int FormatVersion = 1;

        public string Projection { get; set; } = "equal-earth";
        public string Source { get; set; } = string.Empty;
        public MapBounds Bounds { get; set; }
        public List<MapCountry> Countries { get; set; } = new List<MapCountry>();

        private Dictionary<string, MapCountry> _byId;

        public bool TryGet(string iso3, out MapCountry country)
        {
            if (_byId == null || _byId.Count != Countries.Count)
            {
                _byId = new Dictionary<string, MapCountry>(StringComparer.OrdinalIgnoreCase);
                foreach (var c in Countries)
                {
                    _byId[c.Id] = c;
                }
            }

            if (iso3 == null)
            {
                country = null;
                return false;
            }

            return _byId.TryGetValue(iso3, out country);
        }

        public MapCountry Get(string iso3)
        {
            if (TryGet(iso3, out var country))
            {
                return country;
            }

            throw new KeyNotFoundException("No map country '" + iso3 + "'.");
        }

        public int TotalVertices(MapZoomBand band)
        {
            var total = 0;
            foreach (var country in Countries)
            {
                total += country.Lod(band).VertexCount;
            }

            return total;
        }

        public int TotalTriangles(MapZoomBand band)
        {
            var total = 0;
            foreach (var country in Countries)
            {
                total += country.Lod(band).Triangles.Length / 3;
            }

            return total;
        }
    }
}
