using System.Collections.Generic;
using Nation.Core.Map.Geometry;

namespace Nation.Core.Map
{
    /// <summary>
    /// Resolves a tap in map units to a country id. Uses the near-detail polygons with a bounds pre-check
    /// (a tap is rare, so the extra vertices cost nothing noticeable) and prefers the smallest containing
    /// territory so enclaves win over their hosts.
    /// </summary>
    public sealed class MapPicker
    {
        private readonly MapCatalog _catalog;
        private readonly List<List<MapPoint>>[] _rings;

        public MapPicker(MapCatalog catalog)
        {
            _catalog = catalog;
            _rings = new List<List<MapPoint>>[catalog.Countries.Count];
            for (var i = 0; i < catalog.Countries.Count; i++)
            {
                var lod = catalog.Countries[i].Lod(MapZoomBand.Near);
                var rings = new List<List<MapPoint>>(lod.RingCount);
                for (var r = 0; r < lod.RingCount; r++)
                {
                    rings.Add(lod.Ring(r));
                }

                _rings[i] = rings;
            }
        }

        public MapCountry CountryAt(float x, float y, bool selectableOnly = true)
        {
            MapCountry best = null;
            for (var i = 0; i < _catalog.Countries.Count; i++)
            {
                var country = _catalog.Countries[i];
                if (selectableOnly && !country.Selectable)
                {
                    continue;
                }

                if (!country.Bounds.Contains(x, y))
                {
                    continue;
                }

                if (best != null && country.Area >= best.Area)
                {
                    continue;
                }

                var rings = _rings[i];
                for (var r = 0; r < rings.Count; r++)
                {
                    if (PolygonQueries.Contains(rings[r], x, y))
                    {
                        best = country;
                        break;
                    }
                }
            }

            return best;
        }
    }
}
