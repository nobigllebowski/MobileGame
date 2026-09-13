using System.Collections.Generic;
using Nation.Core.Localization;
using Nation.Core.Map;
using Nation.Game.UI.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Nation.Game.Map
{
    /// <summary>
    /// Capital markers as pooled UI elements (dot plus, at near zoom, the city name). Hidden entirely in the far
    /// band; repositioned only on camera change.
    /// </summary>
    public sealed class MapCapitalRenderer
    {
        private const int MaxMarkers = 48;

        private readonly MapService _service;
        private readonly MapCameraController _camera;
        private readonly VisualElement _layer;
        private readonly ILocalizationService _loc;
        private readonly List<VisualElement> _pool = new List<VisualElement>();
        private readonly List<Label> _names = new List<Label>();
        private readonly List<MapCountry> _candidates = new List<MapCountry>();
        private string _playerId;
        private string _selectedId;

        public MapCapitalRenderer(MapService service, MapCameraController camera, VisualElement layer, ILocalizationService loc)
        {
            _service = service;
            _camera = camera;
            _layer = layer;
            _loc = loc;
            _camera.Changed += Refresh;
        }

        public void SetPlayer(string playerId) => _playerId = playerId;

        public void SetSelected(string selectedId)
        {
            _selectedId = selectedId;
            Refresh();
        }

        public void Dispose()
        {
            _camera.Changed -= Refresh;
        }

        public void Refresh()
        {
            var width = _camera.VisibleWidth;
            if (!_service.Zoom.ShowCapitals(width))
            {
                HideFrom(0);
                _service.Stats.VisibleCapitals = 0;
                return;
            }

            var near = _camera.Band == MapZoomBand.Near;
            _candidates.Clear();
            foreach (var country in _service.Catalog.Countries)
            {
                if (!country.HasCapital || !_camera.IsVisible(country.Bounds))
                {
                    continue;
                }

                if (!near && country.LabelRank > 4 && country.Id != _playerId && country.Id != _selectedId)
                {
                    continue;
                }

                _candidates.Add(country);
            }

            if (_candidates.Count > MaxMarkers)
            {
                _candidates.Sort((a, b) => a.LabelRank.CompareTo(b.LabelRank));
                _candidates.RemoveRange(MaxMarkers, _candidates.Count - MaxMarkers);
            }

            var layerBound = _layer.worldBound;
            var used = 0;
            foreach (var country in _candidates)
            {
                var marker = Acquire(used);
                var name = _names[used];
                used++;

                var screen = _camera.MapToScreen(country.CapitalX, country.CapitalY);
                var panel = PanelCoordinates.ScreenToPanel(_layer.panel, screen);

                marker.style.left = panel.x - layerBound.x;
                marker.style.top = panel.y - layerBound.y;
                marker.style.display = DisplayStyle.Flex;
                marker.EnableInClassList("capital--player", country.Id == _playerId);
                marker.EnableInClassList("capital--selected", country.Id == _selectedId);
                var showName = near || country.Id == _selectedId;
                name.style.display = showName ? DisplayStyle.Flex : DisplayStyle.None;
                if (showName)
                {
                    name.text = _loc.Get(country.CapitalKey);
                }
            }

            HideFrom(used);
            _service.Stats.VisibleCapitals = used;
        }

        private void HideFrom(int index)
        {
            for (var i = index; i < _pool.Count; i++)
            {
                _pool[i].style.display = DisplayStyle.None;
            }
        }

        private VisualElement Acquire(int index)
        {
            while (_pool.Count <= index)
            {
                var marker = new VisualElement { pickingMode = PickingMode.Ignore };
                marker.AddToClassList("capital");
                var dot = new VisualElement { pickingMode = PickingMode.Ignore };
                dot.AddToClassList("capital__dot");
                marker.Add(dot);
                var name = new Label { pickingMode = PickingMode.Ignore };
                name.AddToClassList("capital__name");
                marker.Add(name);
                _layer.Add(marker);
                _pool.Add(marker);
                _names.Add(name);
            }

            return _pool[index];
        }
    }
}
