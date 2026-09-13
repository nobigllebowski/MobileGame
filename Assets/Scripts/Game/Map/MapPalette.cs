using Nation.Core.Map;
using Nation.Core.Map.Layers;
using Nation.Game.UI.Core;
using UnityEngine;

namespace Nation.Game.Map
{
    /// <summary>
    /// Colors for the map surface. Restrained slate blues for land; accent reserved for the player, the
    /// selection and the active thematic layer, as the visual direction requires.
    /// </summary>
    public static class MapPalette
    {
        public static readonly Color Ocean = Palette.Hex("#070C1A");
        public static readonly Color OceanDeep = Palette.Hex("#04070F");
        public static readonly Color Land = Palette.Hex("#2A3650");
        public static readonly Color LandSimulated = Palette.Hex("#3A4B6E");
        public static readonly Color LandDependency = Palette.Hex("#263248");
        public static readonly Color LandPlayer = Palette.Hex("#2F6FE0");
        public static readonly Color LandNoData = Palette.Hex("#1F2A40");
        public static readonly Color Border = new Color(0.55f, 0.70f, 0.95f, 0.30f);
        public static readonly Color BorderFar = new Color(0.55f, 0.70f, 0.95f, 0.22f);
        public static readonly Color Graticule = new Color(0.47f, 0.63f, 0.9f, 0.05f);
        public static readonly Color Selection = Palette.Hex("#7FB0FF");
        public static readonly Color SelectionGlow = new Color(0.5f, 0.69f, 1f, 0.28f);

        public static readonly Color GradientLow = Palette.Hex("#243049");
        public static readonly Color GradientMid = Palette.Hex("#3F6AA8");
        public static readonly Color GradientHigh = Palette.Hex("#8FC1FF");
        public static readonly Color GradientGoldHigh = Palette.Hex("#E2C46F");

        public static Color Political(float value)
        {
            if (value >= 1f) return LandPlayer;
            if (value >= 0.5f) return LandSimulated;
            if (value >= 0.25f) return LandDependency;
            return Land;
        }

        public static Color Gradient(float value, bool gold)
        {
            value = Mathf.Clamp01(value);
            var high = gold ? GradientGoldHigh : GradientHigh;
            return value < 0.5f
                ? Color.Lerp(GradientLow, GradientMid, value * 2f)
                : Color.Lerp(GradientMid, high, (value - 0.5f) * 2f);
        }

        /// <summary>Resolves the fill color of a country for a layer, honoring the player highlight on every layer.</summary>
        public static Color ColorFor(IMapLayerProvider layer, MapCountry country, bool isPlayer)
        {
            if (layer.Kind == MapLayerKind.Political)
            {
                layer.TryGetValue(country, out var category);
                return Political(category);
            }

            if (!layer.TryGetValue(country, out var value))
            {
                return isPlayer ? LandSimulated : LandNoData;
            }

            var gold = layer.Kind == MapLayerKind.Economy || layer.Kind == MapLayerKind.Influence;
            var color = Gradient(value, gold);
            return isPlayer ? Color.Lerp(color, LandPlayer, 0.35f) : color;
        }
    }
}
