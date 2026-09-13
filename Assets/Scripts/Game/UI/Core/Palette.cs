using UnityEngine;

namespace Nation.Game.UI.Core
{
    /// <summary>
    /// Colors for code-drawn visuals (flags, icons, charts, map). Mirrors the tokens in Assets/UI/Theme.uss;
    /// keep the two in sync when the palette changes.
    /// </summary>
    public static class Palette
    {
        public static readonly Color Background = Hex("#05080F");
        public static readonly Color Surface = new Color(0.08f, 0.13f, 0.24f, 0.55f);
        public static readonly Color Border = new Color(0.47f, 0.63f, 0.9f, 0.14f);
        public static readonly Color Text = Hex("#EEF3FB");
        public static readonly Color TextSecondary = new Color(0.78f, 0.84f, 0.92f, 0.72f);
        public static readonly Color TextMuted = new Color(0.59f, 0.69f, 0.84f, 0.55f);
        public static readonly Color Accent = Hex("#3B82F6");
        public static readonly Color AccentSoft = new Color(0.23f, 0.51f, 0.96f, 0.22f);
        public static readonly Color Gold = Hex("#D9B45B");
        public static readonly Color Danger = Hex("#E5484D");
        public static readonly Color Positive = Hex("#3DD68C");
        public static readonly Color Warning = Hex("#F5A524");

        public static readonly Color MapOcean = Hex("#070C19");
        public static readonly Color MapGrid = new Color(0.47f, 0.63f, 0.9f, 0.07f);
        public static readonly Color MapLand = new Color(0.16f, 0.24f, 0.4f, 0.55f);
        public static readonly Color MapLandEdge = new Color(0.47f, 0.63f, 0.9f, 0.22f);

        public static Color Hex(string hex)
        {
            return ColorUtility.TryParseHtmlString(hex, out var color) ? color : Color.magenta;
        }

        public static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}
