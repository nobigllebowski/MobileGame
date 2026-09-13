using System;
using Nation.Core.Map;
using UnityEngine;

namespace Nation.Game.Map
{
    /// <summary>
    /// Orthographic camera over the map plane. Pan and zoom requests set a target; LateUpdate damps the camera
    /// toward it and raises Changed only on frames where something actually moved, so idle frames cost nothing.
    /// All public positions are in map units; the world-unit scale is an internal detail.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MapCameraController : MonoBehaviour
    {
        public const float Scale = 10f;
        private const float DampTime = 0.18f;
        private const float SettleEpsilon = 0.00005f;

        private Camera _camera;
        private MapZoomModel _zoom;
        private Vector2 _center;
        private Vector2 _targetCenter;
        private Vector2 _centerVelocity;
        private float _visibleWidth = 5f;
        private float _targetWidth = 5f;
        private float _widthVelocity;
        private bool _animating;
        private bool _dirty;
        private MapZoomBand _band = MapZoomBand.Far;
        private Rect _viewport;
        private bool _hasViewport;

        public MapZoomBand Band => _band;
        public float VisibleWidth => _visibleWidth;
        public Vector2 Center => _center;
        public MapZoomModel Zoom => _zoom;
        public bool IsAnimating => _animating;
        public Camera Camera => _camera;

        public event Action Changed;
        public event Action<MapZoomBand> BandChanged;

        public void Initialize(Camera camera, MapZoomModel zoom)
        {
            _camera = camera;
            _zoom = zoom;
            _camera.orthographic = true;
            _camera.transform.position = new Vector3(0, 0, -10f);
            _camera.transform.rotation = Quaternion.identity;
            _visibleWidth = _targetWidth = zoom.MaxVisibleWidth;
            _center = _targetCenter = new Vector2(zoom.Bounds.CenterX, zoom.Bounds.CenterY);
            _dirty = true;
            Apply();
        }

        /// <summary>Screen-pixel rectangle (bottom-left origin) that the map is actually visible in, between HUD and navigation.</summary>
        public void SetViewport(Rect viewport)
        {
            _viewport = viewport;
            _hasViewport = viewport.width > 0 && viewport.height > 0;
            _dirty = true;
        }

        public void Pan(Vector2 screenDelta)
        {
            var unitsPerPixel = _visibleWidth / Mathf.Max(1f, _camera.pixelWidth);
            _animating = false;
            _center -= screenDelta * unitsPerPixel;
            _targetCenter = _center;
            _dirty = true;
        }

        /// <summary>Zooms by a factor (2 = twice as close) keeping the map point under the anchor fixed.</summary>
        public void ZoomBy(float factor, Vector2 screenAnchor)
        {
            if (factor <= 0)
            {
                return;
            }

            var before = ScreenToMap(screenAnchor);
            _animating = false;
            _visibleWidth = _zoom.ClampVisibleWidth(_visibleWidth / factor);
            _targetWidth = _visibleWidth;
            var after = ScreenToMapWithWidth(screenAnchor, _visibleWidth);
            _center += before - after;
            _targetCenter = _center;
            _dirty = true;
        }

        public void FocusOn(Vector2 mapCenter, float visibleWidth)
        {
            _targetCenter = mapCenter;
            _targetWidth = _zoom.ClampVisibleWidth(visibleWidth);
            _animating = true;
        }

        public void FocusOn(MapCountry country, bool keepZoomIfCloser = true)
        {
            var aspect = ViewportAspect();
            var width = _zoom.FocusWidthFor(country.Bounds, aspect);
            if (keepZoomIfCloser && _visibleWidth < width * 1.15f)
            {
                width = _visibleWidth;
            }

            var center = new Vector2(country.Bounds.CenterX, country.Bounds.CenterY);
            if (country.HasCapital && country.Bounds.Width > 1.5f)
            {
                // Sprawling multi-part countries (Russia, the United States, France) focus on the capital instead of the bounding box center.
                center = new Vector2(country.CapitalX, country.CapitalY);
            }

            FocusOn(center, width);
        }

        public Vector2 ScreenToMap(Vector2 screenPosition)
        {
            return ScreenToMapWithWidth(screenPosition, _visibleWidth);
        }

        public Vector2 MapToScreen(float mapX, float mapY)
        {
            var world = _camera.WorldToScreenPoint(new Vector3(mapX * Scale, mapY * Scale, 0f));
            return new Vector2(world.x, world.y);
        }

        public bool IsVisible(Core.Map.Geometry.MapBounds bounds)
        {
            var visibleHeight = _visibleWidth / Mathf.Max(0.01f, _camera.aspect);
            var view = new Core.Map.Geometry.MapBounds
            {
                MinX = _center.x - _visibleWidth * 0.5f,
                MaxX = _center.x + _visibleWidth * 0.5f,
                MinY = _center.y - visibleHeight * 0.5f,
                MaxY = _center.y + visibleHeight * 0.5f
            };
            return view.Intersects(bounds);
        }

        private Vector2 ScreenToMapWithWidth(Vector2 screenPosition, float width)
        {
            var pixelWidth = Mathf.Max(1f, _camera.pixelWidth);
            var pixelHeight = Mathf.Max(1f, _camera.pixelHeight);
            var unitsPerPixel = width / pixelWidth;
            var x = _center.x + (screenPosition.x - pixelWidth * 0.5f) * unitsPerPixel;
            var y = _center.y + (screenPosition.y - pixelHeight * 0.5f) * unitsPerPixel;
            return new Vector2(x, y);
        }

        private float ViewportAspect()
        {
            if (_hasViewport)
            {
                return _viewport.width / Mathf.Max(1f, _viewport.height);
            }

            return _camera.aspect;
        }

        private void LateUpdate()
        {
            if (_animating)
            {
                _center = Vector2.SmoothDamp(_center, _targetCenter, ref _centerVelocity, DampTime, float.MaxValue, UnityEngine.Time.unscaledDeltaTime);
                _visibleWidth = Mathf.SmoothDamp(_visibleWidth, _targetWidth, ref _widthVelocity, DampTime, float.MaxValue, UnityEngine.Time.unscaledDeltaTime);
                var settled = (_center - _targetCenter).sqrMagnitude < SettleEpsilon && Mathf.Abs(_visibleWidth - _targetWidth) < SettleEpsilon;
                if (settled)
                {
                    _center = _targetCenter;
                    _visibleWidth = _targetWidth;
                    _animating = false;
                }

                _dirty = true;
            }

            if (_dirty)
            {
                Apply();
            }
        }

        private void Apply()
        {
            _dirty = false;
            if (_camera == null || _zoom == null)
            {
                return;
            }

            _visibleWidth = _zoom.ClampVisibleWidth(_visibleWidth);
            var aspect = Mathf.Max(0.01f, _camera.aspect);
            var visibleHeight = _visibleWidth / aspect;

            var cx = _center.x;
            var cy = _center.y;
            _zoom.ClampCenter(ref cx, ref cy, _visibleWidth, visibleHeight);
            _center = new Vector2(cx, cy);
            if (!_animating)
            {
                _targetCenter = _center;
            }

            // Offset so the map center sits in the middle of the visible viewport rather than the whole screen.
            var offset = Vector2.zero;
            if (_hasViewport)
            {
                var unitsPerPixel = _visibleWidth / Mathf.Max(1f, _camera.pixelWidth);
                var screenCenter = new Vector2(_camera.pixelWidth * 0.5f, _camera.pixelHeight * 0.5f);
                var viewportCenter = new Vector2(_viewport.x + _viewport.width * 0.5f, _viewport.y + _viewport.height * 0.5f);
                offset = (screenCenter - viewportCenter) * unitsPerPixel;
            }

            _camera.orthographicSize = visibleHeight * Scale * 0.5f;
            _camera.transform.position = new Vector3((_center.x + offset.x) * Scale, (_center.y + offset.y) * Scale, -10f);

            var band = _zoom.BandFor(_visibleWidth);
            if (band != _band)
            {
                _band = band;
                BandChanged?.Invoke(band);
            }

            Changed?.Invoke();
        }
    }
}
