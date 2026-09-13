using System;
using System.Diagnostics;
using Nation.Core.Map;
using Nation.Core.Map.Layers;
using Nation.Core.Signals;
using UnityEngine;

namespace Nation.Game.Map
{
    /// <summary>
    /// Applies the active thematic layer to the renderer's vertex colors. Recolors on layer change and when the
    /// player country changes; live-data layers refresh at most once per game month, never per frame.
    /// </summary>
    public sealed class MapLayerController : IDisposable
    {
        private const int RefreshEveryDays = 30;

        private readonly MapService _service;
        private readonly MapRenderer _renderer;
        private readonly Func<string> _playerCountry;
        private readonly IDisposable _layerSubscription;
        private readonly IDisposable _tickSubscription;
        private long _lastRefreshTick;

        public MapLayerController(MapService service, MapRenderer renderer, SignalBus signals, Func<string> playerCountry)
        {
            _service = service;
            _renderer = renderer;
            _playerCountry = playerCountry;
            _layerSubscription = signals.Subscribe<MapLayerChangedSignal>(_ => Apply());
            _tickSubscription = signals.Subscribe<TickCompletedSignal>(OnTick);
            Apply();
        }

        public void Apply()
        {
            var stopwatch = Stopwatch.StartNew();
            var layer = _service.ActiveLayerProvider;
            var player = _playerCountry();
            _renderer.SetColors(country => MapPalette.ColorFor(layer, country, string.Equals(country.Id, player, StringComparison.OrdinalIgnoreCase)));
            _service.Stats.LastRecolorMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
        }

        private void OnTick(TickCompletedSignal signal)
        {
            if (_service.ActiveLayerProvider.Style != MapLayerStyle.Gradient)
            {
                return;
            }

            if (signal.TickCount - _lastRefreshTick >= RefreshEveryDays)
            {
                _lastRefreshTick = signal.TickCount;
                Apply();
            }
        }

        public void Dispose()
        {
            _layerSubscription?.Dispose();
            _tickSubscription?.Dispose();
        }
    }
}
