using System.Collections.Generic;
using Nation.Game.UI.Components;
using UnityEngine.UIElements;

namespace Nation.Game.UI.Core
{
    public enum ToastKind
    {
        Info,
        Success,
        Warning,
        Error
    }

    /// <summary>Queued, non-blocking notifications that slide in below the top safe area and leave on their own.</summary>
    public sealed class ToastLayer
    {
        private const long VisibleMs = 2400;
        private const long AnimationMs = 240;
        private const string VisibleClass = "toast--visible";

        private readonly VisualElement _layer;
        private readonly Queue<KeyValuePair<string, ToastKind>> _queue = new Queue<KeyValuePair<string, ToastKind>>();
        private bool _showing;

        public ToastLayer(VisualElement layer)
        {
            _layer = layer;
            _layer.pickingMode = PickingMode.Ignore;
        }

        public void Show(string message, ToastKind kind = ToastKind.Info)
        {
            _queue.Enqueue(new KeyValuePair<string, ToastKind>(message, kind));
            if (!_showing)
            {
                ShowNext();
            }
        }

        private void ShowNext()
        {
            if (_queue.Count == 0)
            {
                _showing = false;
                return;
            }

            _showing = true;
            var entry = _queue.Dequeue();

            var toast = new VisualElement { name = "toast" };
            toast.AddToClassList("toast");
            toast.AddToClassList("toast--" + entry.Value.ToString().ToLowerInvariant());
            toast.pickingMode = PickingMode.Ignore;

            var icon = new IconElement(IconFor(entry.Value)) { Color = ColorFor(entry.Value) };
            icon.AddToClassList("toast__icon");
            toast.Add(icon);

            var label = new Label(entry.Key);
            label.AddToClassList("toast__text");
            toast.Add(label);

            _layer.Add(toast);
            toast.schedule.Execute(() => toast.AddToClassList(VisibleClass));
            toast.schedule.Execute(() => toast.RemoveFromClassList(VisibleClass)).StartingIn(VisibleMs);
            toast.schedule.Execute(() =>
            {
                toast.RemoveFromHierarchy();
                ShowNext();
            }).StartingIn(VisibleMs + AnimationMs);
        }

        private static IconKind IconFor(ToastKind kind)
        {
            switch (kind)
            {
                case ToastKind.Success: return IconKind.Check;
                case ToastKind.Warning: return IconKind.Alert;
                case ToastKind.Error: return IconKind.Alert;
                default: return IconKind.Info;
            }
        }

        private static UnityEngine.Color ColorFor(ToastKind kind)
        {
            switch (kind)
            {
                case ToastKind.Success: return Palette.Positive;
                case ToastKind.Warning: return Palette.Warning;
                case ToastKind.Error: return Palette.Danger;
                default: return Palette.Accent;
            }
        }
    }
}
