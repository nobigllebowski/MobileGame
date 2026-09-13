using UnityEngine;
using UnityEngine.UIElements;

namespace Nation.Game.UI.Core
{
    /// <summary>
    /// Conversions between Unity screen pixels (bottom-left origin, what Camera.WorldToScreenPoint and
    /// Camera.ScreenToWorldPoint use) and UI Toolkit panel units (top-left origin).
    ///
    /// A runtime panel maps the whole screen onto its visual tree with a uniform scale, so the ratio between
    /// Screen size and visualTree.layout is all that is needed. Doing the math here rather than through
    /// RuntimePanelUtils keeps both directions exact inverses of each other and avoids depending on editor
    /// helper overloads that differ between Unity versions.
    /// </summary>
    public static class PanelCoordinates
    {
        /// <summary>Panel units per screen pixel, or 1 when the panel is not laid out yet.</summary>
        public static float UnitsPerPixel(IPanel panel)
        {
            if (panel == null)
            {
                return 1f;
            }

            var layout = panel.visualTree.layout;
            if (layout.width <= 0f || Screen.width <= 0)
            {
                return 1f;
            }

            return layout.width / Screen.width;
        }

        /// <summary>Screen pixels (bottom-left origin) to panel units (top-left origin).</summary>
        public static Vector2 ScreenToPanel(IPanel panel, Vector2 screenPosition)
        {
            if (panel == null)
            {
                return screenPosition;
            }

            var layout = panel.visualTree.layout;
            if (layout.width <= 0f || layout.height <= 0f)
            {
                return screenPosition;
            }

            var x = screenPosition.x / Screen.width * layout.width;
            var y = (Screen.height - screenPosition.y) / Screen.height * layout.height;
            return new Vector2(x, y);
        }

        /// <summary>Panel units (top-left origin) to screen pixels (bottom-left origin).</summary>
        public static Vector2 PanelToScreen(IPanel panel, Vector2 panelPosition)
        {
            if (panel == null)
            {
                return panelPosition;
            }

            var layout = panel.visualTree.layout;
            if (layout.width <= 0f || layout.height <= 0f)
            {
                return panelPosition;
            }

            var x = panelPosition.x / layout.width * Screen.width;
            var y = Screen.height - panelPosition.y / layout.height * Screen.height;
            return new Vector2(x, y);
        }
    }
}
