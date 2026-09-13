using System;
using System.Collections.Generic;
using Nation.Game.UI.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Nation.Game.Map
{
    /// <summary>
    /// Transparent UI element that owns touch and mouse input over the map: one-finger drag, two-finger pinch,
    /// tap, double tap and mouse wheel. Living in the UI tree means sheets, HUD and navigation block it naturally.
    /// Emits screen-pixel coordinates with a bottom-left origin, matching the camera.
    /// </summary>
    public sealed class MapGestureElement : VisualElement
    {
        private const float TapMaxMovement = 12f;
        private const long TapMaxDurationMs = 350;
        private const long DoubleTapWindowMs = 320;
        private const float DoubleTapMaxDistance = 40f;

        private sealed class PointerState
        {
            public int Id;
            public Vector2 StartPanel;
            public Vector2 LastPanel;
            public long StartTime;
            public bool Moved;
        }

        private readonly List<PointerState> _pointers = new List<PointerState>();
        private float _pinchStartDistance;
        private bool _pinching;
        private long _lastTapTime = long.MinValue;
        private Vector2 _lastTapPanel;

        public event Action<Vector2> Dragged;
        public event Action<float, Vector2> Pinched;
        public event Action<Vector2> Tapped;
        public event Action<Vector2> DoubleTapped;
        public event Action<float, Vector2> Wheel;

        public MapGestureElement()
        {
            name = "map-gestures";
            AddToClassList("map-gestures");
            pickingMode = PickingMode.Position;
            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
            RegisterCallback<PointerCancelEvent>(OnPointerCancel);
            RegisterCallback<WheelEvent>(OnWheel);
        }

        /// <summary>Converts a panel-space position to Unity screen pixels with a bottom-left origin.</summary>
        public Vector2 PanelToScreen(Vector2 panelPosition)
        {
            return PanelCoordinates.PanelToScreen(panel, panelPosition);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (Find(evt.pointerId) != null || _pointers.Count >= 2)
            {
                return;
            }

            var position = new Vector2(evt.position.x, evt.position.y);
            _pointers.Add(new PointerState { Id = evt.pointerId, StartPanel = position, LastPanel = position, StartTime = evt.timestamp });
            this.CapturePointer(evt.pointerId);

            if (_pointers.Count == 2)
            {
                _pinching = true;
                _pinchStartDistance = Vector2.Distance(_pointers[0].LastPanel, _pointers[1].LastPanel);
            }

            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            var state = Find(evt.pointerId);
            if (state == null)
            {
                return;
            }

            var position = new Vector2(evt.position.x, evt.position.y);
            var delta = position - state.LastPanel;
            if ((position - state.StartPanel).magnitude > TapMaxMovement)
            {
                state.Moved = true;
            }

            if (_pinching && _pointers.Count == 2)
            {
                state.LastPanel = position;
                var distance = Vector2.Distance(_pointers[0].LastPanel, _pointers[1].LastPanel);
                if (_pinchStartDistance > 1f && distance > 1f)
                {
                    var factor = distance / _pinchStartDistance;
                    _pinchStartDistance = distance;
                    var mid = (_pointers[0].LastPanel + _pointers[1].LastPanel) * 0.5f;
                    Pinched?.Invoke(factor, PanelToScreen(mid));
                }
            }
            else if (_pointers.Count == 1)
            {
                state.LastPanel = position;
                var screenDelta = PanelToScreen(position) - PanelToScreen(position - delta);
                if (screenDelta.sqrMagnitude > 0)
                {
                    Dragged?.Invoke(screenDelta);
                }
            }

            evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            var state = Find(evt.pointerId);
            if (state == null)
            {
                return;
            }

            if (this.HasPointerCapture(evt.pointerId))
            {
                this.ReleasePointer(evt.pointerId);
            }

            _pointers.Remove(state);
            if (_pointers.Count < 2)
            {
                _pinching = false;
            }

            var duration = evt.timestamp - state.StartTime;
            if (!state.Moved && duration <= TapMaxDurationMs && !_pinching && _pointers.Count == 0)
            {
                var panelPosition = new Vector2(evt.position.x, evt.position.y);
                var screen = PanelToScreen(panelPosition);
                var isDouble = evt.timestamp - _lastTapTime <= DoubleTapWindowMs && Vector2.Distance(panelPosition, _lastTapPanel) <= DoubleTapMaxDistance;
                if (isDouble)
                {
                    _lastTapTime = long.MinValue;
                    DoubleTapped?.Invoke(screen);
                }
                else
                {
                    _lastTapTime = evt.timestamp;
                    _lastTapPanel = panelPosition;
                    Tapped?.Invoke(screen);
                }
            }

            evt.StopPropagation();
        }

        private void OnPointerCancel(PointerCancelEvent evt)
        {
            var state = Find(evt.pointerId);
            if (state == null)
            {
                return;
            }

            if (this.HasPointerCapture(evt.pointerId))
            {
                this.ReleasePointer(evt.pointerId);
            }

            _pointers.Remove(state);
            _pinching = _pointers.Count == 2;
        }

        private void OnWheel(WheelEvent evt)
        {
            var factor = evt.delta.y < 0 ? 1.18f : 1f / 1.18f;
            Wheel?.Invoke(factor, PanelToScreen(new Vector2(evt.mousePosition.x, evt.mousePosition.y)));
            evt.StopPropagation();
        }

        private PointerState Find(int pointerId)
        {
            for (var i = 0; i < _pointers.Count; i++)
            {
                if (_pointers[i].Id == pointerId)
                {
                    return _pointers[i];
                }
            }

            return null;
        }
    }
}
