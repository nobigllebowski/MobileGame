using UnityEngine;
using UnityEngine.UIElements;

namespace Nation.Game.UI.Core
{
    /// <summary>
    /// Pads its content by the device safe area (notch, Dynamic Island, punch-hole camera, gesture bar).
    /// Converts screen pixels to panel units, so it stays correct under any panel scale mode.
    /// Recomputed whenever the panel geometry changes (rotation, window resize, Device Simulator).
    /// </summary>
    public sealed class SafeAreaElement : VisualElement
    {
        private const float MinimumBottomInset = 12f;

        public float TopInset { get; private set; }
        public float BottomInset { get; private set; }

        public SafeAreaElement()
        {
            name = "safe-area";
            AddToClassList("safe-area");
            RegisterCallback<GeometryChangedEvent>(_ => Apply());
            RegisterCallback<AttachToPanelEvent>(_ => Apply());
        }

        public void Apply()
        {
            if (panel == null)
            {
                return;
            }

            var safe = Screen.safeArea;
            var screenWidth = (float)Screen.width;
            var screenHeight = (float)Screen.height;
            if (screenWidth <= 0 || screenHeight <= 0)
            {
                return;
            }

            var scale = PanelUnitsPerPixel();
            var left = safe.xMin * scale;
            var right = (screenWidth - safe.xMax) * scale;
            var top = (screenHeight - safe.yMax) * scale;
            var bottom = safe.yMin * scale;

            if (bottom < MinimumBottomInset)
            {
                bottom = MinimumBottomInset;
            }

            TopInset = top;
            BottomInset = bottom;

            style.paddingLeft = left;
            style.paddingRight = right;
            style.paddingTop = top;
            style.paddingBottom = bottom;
        }

        private float PanelUnitsPerPixel()
        {
            var origin = RuntimePanelUtils.ScreenToPanel(panel, Vector2.zero);
            var probe = RuntimePanelUtils.ScreenToPanel(panel, new Vector2(100f, 0f));
            var perPixel = Mathf.Abs(probe.x - origin.x) / 100f;
            return perPixel > 0.0001f ? perPixel : 1f;
        }
    }
}
