using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Nation.Game.UI.Core
{
    public enum SheetState
    {
        Peek,
        Half,
        Expanded
    }

    /// <summary>
    /// Reusable draggable bottom sheet with PEEK, HALF and EXPANDED snap points, velocity-aware release and
    /// optional tap-outside dismissal. Content is supplied by the caller (countries, buildings, events...).
    /// Position is animated through the `top` style with a USS transition; the transition is switched off
    /// while a finger is on the sheet so it follows the drag exactly.
    /// </summary>
    public sealed class BottomSheet : VisualElement
    {
        public const long AnimationMs = 280;

        private const string DraggingClass = "sheet--dragging";
        private const float DismissVelocity = 900f;
        private const float SnapVelocity = 450f;

        private readonly VisualElement _grabber;
        private readonly VisualElement _header;
        private readonly ScrollView _body;

        private float _peekFraction = 0.34f;
        private float _halfFraction = 0.6f;
        private float _expandedFraction = 0.9f;

        private float _layerHeight;
        private bool _dragging;
        private int _pointerId = -1;
        private float _dragStartY;
        private float _dragStartTop;
        private float _lastY;
        private long _lastTime;
        private float _velocity;

        public SheetState State { get; private set; } = SheetState.Peek;
        public bool Dismissible { get; set; } = true;
        public bool AllowExpanded { get; set; } = true;
        public bool AllowHalf { get; set; } = true;

        public VisualElement Header => _header;
        public VisualElement Body => _body.contentContainer;

        public event Action<SheetState> StateChanged;
        public event Action Dismissed;

        public BottomSheet()
        {
            name = "bottom-sheet";
            AddToClassList("sheet");
            pickingMode = PickingMode.Position;

            _grabber = new VisualElement { name = "sheet-grabber" };
            _grabber.AddToClassList("sheet__grabber");
            var handle = new VisualElement();
            handle.AddToClassList("sheet__handle");
            _grabber.Add(handle);
            Add(_grabber);

            _header = new VisualElement { name = "sheet-header" };
            _header.AddToClassList("sheet__header");
            Add(_header);

            _body = new ScrollView(ScrollViewMode.Vertical) { name = "sheet-body" };
            _body.AddToClassList("sheet__body");
            _body.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            _body.verticalScrollerVisibility = ScrollerVisibility.Hidden;
            Add(_body);

            RegisterDrag(_grabber);
            RegisterDrag(_header);
        }

        public void SetSnapPoints(float peek, float half, float expanded)
        {
            _peekFraction = Mathf.Clamp01(peek);
            _halfFraction = Mathf.Clamp01(half);
            _expandedFraction = Mathf.Clamp01(expanded);
        }

        internal void Layout(float layerHeight, SheetState initial)
        {
            _layerHeight = layerHeight;
            style.height = layerHeight * _expandedFraction;
            style.top = layerHeight;
            State = initial;
            schedule.Execute(() => SnapTo(initial, false));
        }

        internal void Relayout(float layerHeight)
        {
            _layerHeight = layerHeight;
            style.height = layerHeight * _expandedFraction;
            SnapTo(State, false);
        }

        public void SnapTo(SheetState state, bool notify = true)
        {
            if (state == SheetState.Expanded && !AllowExpanded) state = SheetState.Half;
            if (state == SheetState.Half && !AllowHalf) state = SheetState.Peek;

            var changed = state != State;
            State = state;
            RemoveFromClassList(DraggingClass);
            style.top = _layerHeight - VisibleHeight(state);
            if (changed && notify)
            {
                StateChanged?.Invoke(state);
            }
        }

        internal void PlayDismiss(Action completed)
        {
            RemoveFromClassList(DraggingClass);
            style.top = _layerHeight;
            schedule.Execute(() => completed?.Invoke()).StartingIn(AnimationMs);
        }

        public void RequestDismiss()
        {
            Dismissed?.Invoke();
        }

        private float VisibleHeight(SheetState state)
        {
            switch (state)
            {
                case SheetState.Expanded: return _layerHeight * _expandedFraction;
                case SheetState.Half: return _layerHeight * _halfFraction;
                default: return _layerHeight * _peekFraction;
            }
        }

        private void RegisterDrag(VisualElement handle)
        {
            handle.RegisterCallback<PointerDownEvent>(OnPointerDown);
            handle.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            handle.RegisterCallback<PointerUpEvent>(OnPointerUp);
            handle.RegisterCallback<PointerCancelEvent>(OnPointerCancel);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (_dragging)
            {
                return;
            }

            _dragging = true;
            _pointerId = evt.pointerId;
            _dragStartY = evt.position.y;
            _dragStartTop = resolvedStyle.top;
            _lastY = _dragStartY;
            _lastTime = evt.timestamp;
            _velocity = 0f;
            AddToClassList(DraggingClass);
            ((VisualElement)evt.currentTarget).CapturePointer(_pointerId);
            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!_dragging || evt.pointerId != _pointerId)
            {
                return;
            }

            var y = evt.position.y;
            var dt = (evt.timestamp - _lastTime) / 1000f;
            if (dt > 0.0005f)
            {
                var instant = (y - _lastY) / dt;
                _velocity = Mathf.Lerp(_velocity, instant, 0.6f);
            }

            _lastY = y;
            _lastTime = evt.timestamp;

            var minTop = _layerHeight - VisibleHeight(SheetState.Expanded);
            var maxTop = _layerHeight;
            var top = Mathf.Clamp(_dragStartTop + (y - _dragStartY), minTop, maxTop);
            style.top = top;
            evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!_dragging || evt.pointerId != _pointerId)
            {
                return;
            }

            FinishDrag((VisualElement)evt.currentTarget);
            evt.StopPropagation();
        }

        private void OnPointerCancel(PointerCancelEvent evt)
        {
            if (!_dragging || evt.pointerId != _pointerId)
            {
                return;
            }

            FinishDrag((VisualElement)evt.currentTarget);
        }

        private void FinishDrag(VisualElement handle)
        {
            _dragging = false;
            if (handle.HasPointerCapture(_pointerId))
            {
                handle.ReleasePointer(_pointerId);
            }

            _pointerId = -1;

            var currentTop = resolvedStyle.top;
            var visible = _layerHeight - currentTop;

            if (_velocity > DismissVelocity && Dismissible && State == SheetState.Peek)
            {
                RequestDismiss();
                return;
            }

            if (_velocity > SnapVelocity)
            {
                if (State == SheetState.Expanded) SnapTo(AllowHalf ? SheetState.Half : SheetState.Peek);
                else if (State == SheetState.Half) SnapTo(SheetState.Peek);
                else if (Dismissible) RequestDismiss();
                else SnapTo(SheetState.Peek);
                return;
            }

            if (_velocity < -SnapVelocity)
            {
                if (State == SheetState.Peek) SnapTo(AllowHalf ? SheetState.Half : (AllowExpanded ? SheetState.Expanded : SheetState.Peek));
                else SnapTo(AllowExpanded ? SheetState.Expanded : SheetState.Half);
                return;
            }

            if (Dismissible && visible < VisibleHeight(SheetState.Peek) * 0.55f)
            {
                RequestDismiss();
                return;
            }

            SnapTo(Nearest(visible));
        }

        private SheetState Nearest(float visible)
        {
            var best = SheetState.Peek;
            var bestDistance = Mathf.Abs(visible - VisibleHeight(SheetState.Peek));

            if (AllowHalf)
            {
                var d = Mathf.Abs(visible - VisibleHeight(SheetState.Half));
                if (d < bestDistance) { bestDistance = d; best = SheetState.Half; }
            }

            if (AllowExpanded)
            {
                var d = Mathf.Abs(visible - VisibleHeight(SheetState.Expanded));
                if (d < bestDistance) { best = SheetState.Expanded; }
            }

            return best;
        }
    }

    /// <summary>Hosts one bottom sheet at a time, with a scrim that dismisses it when appropriate.</summary>
    public sealed class SheetLayer
    {
        private const string VisibleClass = "sheet-layer--visible";

        private readonly VisualElement _layer;
        private readonly VisualElement _scrim;
        private BottomSheet _sheet;

        public BottomSheet Current => _sheet;
        public bool IsOpen => _sheet != null;

        public SheetLayer(VisualElement layer)
        {
            _layer = layer;
            _layer.style.display = DisplayStyle.None;
            _layer.pickingMode = PickingMode.Ignore;

            _scrim = new VisualElement { name = "sheet-scrim" };
            _scrim.AddToClassList("sheet-scrim");
            _scrim.RegisterCallback<ClickEvent>(_ =>
            {
                if (_sheet != null && _sheet.Dismissible)
                {
                    Dismiss();
                }
            });
            _layer.Add(_scrim);

            _layer.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                if (_sheet != null && evt.newRect.height > 0)
                {
                    _sheet.Relayout(evt.newRect.height);
                }
            });
        }

        public void Show(BottomSheet sheet, SheetState initial = SheetState.Peek)
        {
            if (_sheet != null)
            {
                _sheet.RemoveFromHierarchy();
            }

            _sheet = sheet;
            sheet.Dismissed += Dismiss;
            _layer.Add(sheet);
            _layer.pickingMode = PickingMode.Position;
            _layer.style.display = DisplayStyle.Flex;

            var height = _layer.resolvedStyle.height;
            if (height <= 0 || float.IsNaN(height))
            {
                height = _layer.panel != null ? _layer.panel.visualTree.resolvedStyle.height : 844f;
            }

            sheet.Layout(height, initial);
            _layer.schedule.Execute(() => _layer.AddToClassList(VisibleClass));
        }

        public void Dismiss()
        {
            if (_sheet == null)
            {
                return;
            }

            var sheet = _sheet;
            _sheet = null;
            _layer.RemoveFromClassList(VisibleClass);
            _layer.pickingMode = PickingMode.Ignore;
            sheet.PlayDismiss(() =>
            {
                sheet.RemoveFromHierarchy();
                if (_sheet == null)
                {
                    _layer.style.display = DisplayStyle.None;
                }
            });
        }
    }
}
