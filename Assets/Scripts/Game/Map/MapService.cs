using System;
using System.Diagnostics;
using Nation.Core.Map;
using Nation.Core.Map.Layers;
using Nation.Core.Models;
using Nation.Core.Signals;
using UnityEngine;

namespace Nation.Game.Map
{
    /// <summary>
    /// Game-wide map state: the loaded catalog, picking, zoom rules, thematic layers, the active layer and the
    /// selected country. Publishes selection and layer signals; renderers and UI react to those.
    /// </summary>
    public sealed class MapService
    {
        private readonly SignalBus _signals;

        public MapCatalog Catalog { get; }
        public MapPicker Picker { get; }
        public MapZoomModel Zoom { get; }
        public MapLayerSet Layers { get; }
        public MapPerformanceStats Stats { get; } = new MapPerformanceStats();
        public MapLayerKind ActiveLayer { get; private set; } = MapLayerKind.Political;
        public string SelectedCountryId { get; private set; }
        public bool IsLoaded => Catalog != null && Catalog.Countries.Count > 0;
        public double LoadMilliseconds { get; }

        public MapService(TextAsset mapBytes, Func<WorldState> world, SignalBus signals)
        {
            _signals = signals ?? throw new ArgumentNullException(nameof(signals));

            var stopwatch = Stopwatch.StartNew();
            Catalog = Load(mapBytes);
            Picker = new MapPicker(Catalog);
            Zoom = new MapZoomModel(Catalog.Bounds);
            Layers = new MapLayerSet(world);
            LoadMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
        }

        public IMapLayerProvider ActiveLayerProvider => Layers.Get(ActiveLayer);

        public MapCountry SelectedCountry => SelectedCountryId != null && Catalog.TryGet(SelectedCountryId, out var country) ? country : null;

        public void SetLayer(MapLayerKind kind)
        {
            if (ActiveLayer == kind)
            {
                return;
            }

            ActiveLayer = kind;
            _signals.Publish(new MapLayerChangedSignal(kind));
        }

        public void Select(string iso3)
        {
            if (iso3 != null && !Catalog.TryGet(iso3, out _))
            {
                UnityEngine.Debug.LogWarning("[Map] Cannot select unknown country '" + iso3 + "'.");
                return;
            }

            if (string.Equals(SelectedCountryId, iso3, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            SelectedCountryId = iso3;
            _signals.Publish(new CountrySelectedSignal(iso3));
        }

        public void Deselect()
        {
            Select(null);
        }

        public MapCountry CountryAt(float mapX, float mapY)
        {
            return Picker.CountryAt(mapX, mapY);
        }

        private static MapCatalog Load(TextAsset mapBytes)
        {
            try
            {
                if (mapBytes == null)
                {
                    throw new InvalidOperationException("no map catalog assigned in the Game Data Catalog");
                }

                var catalog = MapCatalogSerializer.Read(mapBytes.bytes);
                UnityEngine.Debug.Log("[Map] Loaded " + catalog.Countries.Count + " territories (" + catalog.Source + ").");
                return catalog;
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogError("[Map] Map catalog failed to load: " + exception.Message + ". Run Nation > Map > Import Natural Earth.");
                var empty = new MapCatalog();
                empty.Bounds = new Core.Map.Geometry.MapBounds { MinX = -2.7f, MinY = -1.3f, MaxX = 2.7f, MaxY = 1.3f };
                return empty;
            }
        }
    }
}
