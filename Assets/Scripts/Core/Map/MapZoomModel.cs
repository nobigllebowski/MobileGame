using System;
using Nation.Core.Map.Geometry;

namespace Nation.Core.Map
{
    /// <summary>
    /// Zoom bands, camera limits and label rules expressed in map units, independent of any camera API.
    /// "Visible width" is the width of the world that fits on screen; zoom bands are thresholds on it.
    /// </summary>
    public sealed class MapZoomModel
    {
        /// <summary>Width of the full Equal Earth world in map units.</summary>
        public const float WorldWidth = 2f * (float)EqualEarthProjection.MaxX;

        public float MinVisibleWidth { get; }
        public float MaxVisibleWidth { get; }
        public float FarThreshold { get; }
        public float MidThreshold { get; }
        public MapBounds Bounds { get; }

        public MapZoomModel(MapBounds bounds, float minVisibleWidth = 0.14f, float farThreshold = 2.4f, float midThreshold = 0.8f)
        {
            Bounds = bounds;
            MinVisibleWidth = minVisibleWidth;
            MaxVisibleWidth = Math.Max(bounds.Width, bounds.Height) * 1.08f;
            FarThreshold = farThreshold;
            MidThreshold = midThreshold;
        }

        public float ClampVisibleWidth(float visibleWidth)
        {
            if (visibleWidth < MinVisibleWidth) return MinVisibleWidth;
            if (visibleWidth > MaxVisibleWidth) return MaxVisibleWidth;
            return visibleWidth;
        }

        public MapZoomBand BandFor(float visibleWidth)
        {
            if (visibleWidth > FarThreshold) return MapZoomBand.Far;
            if (visibleWidth > MidThreshold) return MapZoomBand.Mid;
            return MapZoomBand.Near;
        }

        /// <summary>Approximate web-map zoom level for the visible width, to reuse Natural Earth's MIN_LABEL values.</summary>
        public static float WebZoomFor(float visibleWidth)
        {
            if (visibleWidth <= 0)
            {
                return 20f;
            }

            return (float)(Math.Log(WorldWidth / visibleWidth, 2.0) + 0.6);
        }

        public bool ShowLabel(MapCountry country, float visibleWidth)
        {
            var band = BandFor(visibleWidth);
            if (band == MapZoomBand.Far)
            {
                return country.LabelRank <= 2;
            }

            var zoom = WebZoomFor(visibleWidth);
            return zoom >= country.MinLabelZoom - 1.4f;
        }

        public bool ShowCapitals(float visibleWidth) => BandFor(visibleWidth) != MapZoomBand.Far;

        /// <summary>Keeps the camera center inside the world so empty space never fills more than a margin.</summary>
        public void ClampCenter(ref float centerX, ref float centerY, float visibleWidth, float visibleHeight)
        {
            var halfW = visibleWidth * 0.5f;
            var halfH = visibleHeight * 0.5f;
            var marginX = visibleWidth * 0.25f;
            var marginY = visibleHeight * 0.25f;

            var minX = Bounds.MinX + halfW - marginX;
            var maxX = Bounds.MaxX - halfW + marginX;
            var minY = Bounds.MinY + halfH - marginY;
            var maxY = Bounds.MaxY - halfH + marginY;

            centerX = minX > maxX ? Bounds.CenterX : Clamp(centerX, minX, maxX);
            centerY = minY > maxY ? Bounds.CenterY : Clamp(centerY, minY, maxY);
        }

        /// <summary>Visible width that frames a country with padding, respecting the zoom limits.</summary>
        public float FocusWidthFor(MapBounds target, float viewportAspect, float padding = 1.6f)
        {
            var width = Math.Max(target.Width, 0.05f) * padding;
            var heightAsWidth = Math.Max(target.Height, 0.05f) * padding * viewportAspect;
            return ClampVisibleWidth(Math.Max(width, heightAsWidth));
        }

        private static float Clamp(float value, float min, float max) => value < min ? min : value > max ? max : value;
    }
}
