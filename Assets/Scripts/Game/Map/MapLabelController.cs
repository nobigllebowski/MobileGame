using System.Collections.Generic;
using System.Diagnostics;
using Nation.Core.Localization;
using Nation.Core.Map;
using Nation.Game.UI.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Nation.Game.Map
{
    /// <summary>
    /// Country name labels drawn as pooled UI Toolkit labels above the map. Repositioned only when the camera
    /// reports a change; candidates are filtered by zoom band, Natural Earth label rank and view bounds, then
    /// capped so the far view stays calm. Text is resolved once per label and cached.
    /// </summary>
    public sealed class MapLabelController
    {
        private const int MaxLabels = 64;

        private readonly MapService _service;
        private readonly MapCameraController _camera;
        private readonly VisualElement _layer;
        private readonly ILocalizationService _loc;
        private readonly List<Label> _pool = new List<Label>();
        private readonly Dictionary<string, string> _textCache = new Dictionary<string, string>();
        private readonly List<MapCountry> _candidates = new List<MapCountry>();
        private string _playerId;

        public MapLabelController(MapService service, MapCameraController camera, VisualElement layer, ILocalizationService loc)
        {
            _service = service;
            _camera = camera;
            _layer = layer;
            _loc = loc;
            _camera.Changed += Refresh;
        }

        public void SetPlayer(string playerId)
        {
            _playerId = playerId;
        }

        public void Dispose()
        {
            _camera.Changed -= Refresh;
        }

        public void Refresh()
        {
            var stopwatch = Stopwatch.StartNew();
            var width = _camera.VisibleWidth;
            var band = _camera.Band;
            var zoom = _service.Zoom;

            _candidates.Clear();
            foreach (var country in _service.Catalog.Countries)
            {
                if (!country.Selectable || !zoom.ShowLabel(country, width) || !_camera.IsVisible(country.Bounds))
                {
                    continue;
                }

                _candidates.Add(country);
            }

            if (_candidates.Count > MaxLabels)
            {
                _candidates.Sort((a, b) => a.LabelRank != b.LabelRank ? a.LabelRank.CompareTo(b.LabelRank) : b.Area.CompareTo(a.Area));
                _candidates.RemoveRange(MaxLabels, _candidates.Count - MaxLabels);
            }

            var layerBound = _layer.worldBound;
            var used = 0;
            for (var i = 0; i < _candidates.Count; i++)
            {
                var country = _candidates[i];
                var label = Acquire(used++);
                var screen = _camera.MapToScreen(country.LabelX, country.LabelY);
                var panel = PanelCoordinates.ScreenToPanel(_layer.panel, screen);

                label.text = TextFor(country);
                label.EnableInClassList("map-label--player", country.Id == _playerId);
                label.EnableInClassList("map-label--major", country.LabelRank <= 2);
                label.EnableInClassList("map-label--near", band == MapZoomBand.Near);
                label.style.left = panel.x - layerBound.x;
                label.style.top = panel.y - layerBound.y;
                label.style.display = DisplayStyle.Flex;
            }

            for (var i = used; i < _pool.Count; i++)
            {
                _pool[i].style.display = DisplayStyle.None;
            }

            _service.Stats.VisibleLabels = used;
            _service.Stats.LastLabelUpdateMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
            _service.Stats.LabelUpdatesThisSecond++;
        }

        private string TextFor(MapCountry country)
        {
            if (!_textCache.TryGetValue(country.Id, out var text))
            {
                text = _loc.Get(country.NameKey);
                _textCache[country.Id] = text;
            }

            return text;
        }

        private Label Acquire(int index)
        {
            while (_pool.Count <= index)
            {
                var label = new Label { pickingMode = PickingMode.Ignore };
                label.AddToClassList("map-label");
                _layer.Add(label);
                _pool.Add(label);
            }

            return _pool[index];
        }
    }
}
