using Nation.Core.Map;
using UnityEngine;

namespace Nation.Game.Map
{
    /// <summary>
    /// Turns taps into selection: resolves the tapped map point through the picker (ISO3 only), updates the
    /// service, and focuses the camera. Double tap zooms toward the tapped point; the home action returns to the
    /// player's country.
    /// </summary>
    public sealed class CountrySelectionController
    {
        private readonly MapService _service;
        private readonly MapCameraController _camera;
        private readonly MapGestureElement _gestures;

        public CountrySelectionController(MapService service, MapCameraController camera, MapGestureElement gestures)
        {
            _service = service;
            _camera = camera;
            _gestures = gestures;
            _gestures.Tapped += OnTapped;
            _gestures.DoubleTapped += OnDoubleTapped;
            _gestures.Dragged += delta => _camera.Pan(delta);
            _gestures.Pinched += (factor, anchor) => _camera.ZoomBy(factor, anchor);
            _gestures.Wheel += (factor, anchor) => _camera.ZoomBy(factor, anchor);
        }

        public void Dispose()
        {
            _gestures.Tapped -= OnTapped;
            _gestures.DoubleTapped -= OnDoubleTapped;
        }

        public void SelectAndFocus(string iso3)
        {
            if (!_service.Catalog.TryGet(iso3, out var country))
            {
                return;
            }

            _service.Select(country.Id);
            _camera.FocusOn(country);
        }

        public void ReturnHome(string playerCountryId)
        {
            if (_service.Catalog.TryGet(playerCountryId, out var country))
            {
                _service.Select(country.Id);
                _camera.FocusOn(country, false);
            }
        }

        private void OnTapped(Vector2 screenPosition)
        {
            var point = _camera.ScreenToMap(screenPosition);
            var country = _service.CountryAt(point.x, point.y);
            if (country == null)
            {
                _service.Deselect();
                return;
            }

            _service.Select(country.Id);
            _camera.FocusOn(country);
        }

        private void OnDoubleTapped(Vector2 screenPosition)
        {
            var selected = _service.SelectedCountry;
            if (selected != null)
            {
                var point = _camera.ScreenToMap(screenPosition);
                if (_service.CountryAt(point.x, point.y) == selected)
                {
                    _camera.FocusOn(selected, false);
                    return;
                }
            }

            var target = _camera.ScreenToMap(screenPosition);
            _camera.FocusOn(target, _camera.VisibleWidth * 0.5f);
        }
    }
}
