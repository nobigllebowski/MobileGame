using System;
using System.Collections.Generic;
using Nation.Core.Countries;
using Nation.Core.Map;
using Nation.Game.UI.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Nation.Game.UI.Components
{
    /// <summary>
    /// Placeholder world view: ocean, latitude grid, coarse landmass silhouettes and capital markers, all vector.
    /// Keeps the same contract the real map will honor (SetCountries, Select, PlayerCountryId, CountryTapped),
    /// so the world tab does not change when Natural Earth polygons and pan/zoom arrive.
    /// </summary>
    public sealed class WorldMapView : VisualElement
    {
        private const double NorthLimit = 84.0;
        private const double SouthLimit = -58.0;

        private readonly IMapDataProvider _mapData;
        private readonly VisualElement _markerLayer;
        private readonly Dictionary<string, VisualElement> _markers = new Dictionary<string, VisualElement>();
        private readonly List<CountryDefinition> _countries = new List<CountryDefinition>();
        private readonly Func<CountryDefinition, string> _labelFor;
        private bool _pulseOn;

        public string PlayerCountryId { get; private set; }
        public string SelectedCountryId { get; private set; }

        public event Action<CountryDefinition> CountryTapped;

        public WorldMapView(IMapDataProvider mapData, Func<CountryDefinition, string> labelFor)
        {
            _mapData = mapData;
            _labelFor = labelFor;
            name = "world-map";
            AddToClassList("map");
            generateVisualContent += OnGenerate;

            _markerLayer = new VisualElement { name = "markers" };
            _markerLayer.AddToClassList("map__markers");
            _markerLayer.pickingMode = PickingMode.Ignore;
            Add(_markerLayer);

            RegisterCallback<GeometryChangedEvent>(_ => PositionMarkers());
            RegisterCallback<AttachToPanelEvent>(_ => schedule.Execute(TogglePulse).Every(900));
        }

        public void SetCountries(IReadOnlyList<CountryDefinition> countries, string playerCountryId)
        {
            _countries.Clear();
            _countries.AddRange(countries);
            PlayerCountryId = playerCountryId;
            _markerLayer.Clear();
            _markers.Clear();

            foreach (var country in _countries)
            {
                var captured = country;
                var marker = new VisualElement { name = "marker-" + country.Id };
                marker.AddToClassList("marker");
                var dot = new VisualElement();
                dot.AddToClassList("marker__dot");
                marker.Add(dot);
                var label = new Label(_labelFor(country));
                label.AddToClassList("marker__label");
                marker.Add(label);
                marker.RegisterCallback<ClickEvent>(evt =>
                {
                    evt.StopPropagation();
                    Select(captured.Id);
                    CountryTapped?.Invoke(captured);
                });
                marker.EnableInClassList("marker--player", country.Id == playerCountryId);
                _markerLayer.Add(marker);
                _markers[country.Id] = marker;
            }

            Select(playerCountryId);
            PositionMarkers();
        }

        public void Select(string countryId)
        {
            SelectedCountryId = countryId;
            foreach (var pair in _markers)
            {
                pair.Value.EnableInClassList("marker--selected", pair.Key == countryId);
            }
        }

        public Vector2 Project(double latitude, double longitude)
        {
            var rect = contentRect;
            var x = (float)(MapProjection.X(longitude) * rect.width);
            var clampedLatitude = Math.Max(SouthLimit, Math.Min(NorthLimit, latitude));
            var y = (float)((NorthLimit - clampedLatitude) / (NorthLimit - SouthLimit) * rect.height);
            return new Vector2(x, y);
        }

        private void PositionMarkers()
        {
            var rect = contentRect;
            if (rect.width <= 0 || rect.height <= 0)
            {
                return;
            }

            foreach (var country in _countries)
            {
                if (!_markers.TryGetValue(country.Id, out var marker))
                {
                    continue;
                }

                var point = Project(country.CapitalLatitude, country.CapitalLongitude);
                marker.style.left = point.x;
                marker.style.top = point.y;
            }
        }

        private void TogglePulse()
        {
            _pulseOn = !_pulseOn;
            if (PlayerCountryId != null && _markers.TryGetValue(PlayerCountryId, out var marker))
            {
                marker.EnableInClassList("marker--pulse", _pulseOn);
            }
        }

        private void OnGenerate(MeshGenerationContext context)
        {
            var rect = contentRect;
            if (rect.width <= 0 || rect.height <= 0)
            {
                return;
            }

            var p = context.painter2D;

            p.strokeColor = Palette.MapGrid;
            p.lineWidth = 1f;
            for (var lon = -150; lon <= 150; lon += 30)
            {
                var x = (float)(MapProjection.X(lon) * rect.width);
                p.BeginPath();
                p.MoveTo(new Vector2(x, 0));
                p.LineTo(new Vector2(x, rect.height));
                p.Stroke();
            }

            for (var lat = -45; lat <= 75; lat += 30)
            {
                var y = Project(lat, 0).y;
                p.BeginPath();
                p.MoveTo(new Vector2(0, y));
                p.LineTo(new Vector2(rect.width, y));
                p.Stroke();
            }

            p.lineWidth = 1f;
            p.lineJoin = LineJoin.Round;
            foreach (var landmass in _mapData.Landmasses)
            {
                var points = landmass.Points;
                if (points.Count < 3)
                {
                    continue;
                }

                p.BeginPath();
                for (var i = 0; i < points.Count; i++)
                {
                    var point = Project(points[i].Latitude, points[i].Longitude);
                    if (i == 0) p.MoveTo(point);
                    else p.LineTo(point);
                }

                p.ClosePath();
                p.fillColor = Palette.MapLand;
                p.Fill();
                p.strokeColor = Palette.MapLandEdge;
                p.Stroke();
            }
        }
    }
}
