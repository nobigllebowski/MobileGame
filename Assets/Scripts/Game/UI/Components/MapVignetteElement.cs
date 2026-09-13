using Nation.Game.UI.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Nation.Game.UI.Components
{
    /// <summary>Soft darkening toward the top and bottom edges of the map viewport; static, redrawn only on resize.</summary>
    public sealed class MapVignetteElement : VisualElement
    {
        private const int Steps = 14;

        public MapVignetteElement()
        {
            name = "map-vignette";
            AddToClassList("map-vignette");
            pickingMode = PickingMode.Ignore;
            generateVisualContent += OnGenerate;
        }

        private void OnGenerate(MeshGenerationContext context)
        {
            var rect = contentRect;
            if (rect.width <= 0 || rect.height <= 0)
            {
                return;
            }

            var p = context.painter2D;
            var band = rect.height * 0.18f;
            for (var i = 0; i < Steps; i++)
            {
                var t = (float)i / Steps;
                var alpha = 0.055f * (1f - t);
                var thickness = band / Steps;
                p.fillColor = Palette.WithAlpha(Palette.Background, alpha);

                p.BeginPath();
                p.MoveTo(new Vector2(0, i * thickness));
                p.LineTo(new Vector2(rect.width, i * thickness));
                p.LineTo(new Vector2(rect.width, (i + 1) * thickness));
                p.LineTo(new Vector2(0, (i + 1) * thickness));
                p.ClosePath();
                p.Fill();

                p.BeginPath();
                p.MoveTo(new Vector2(0, rect.height - i * thickness));
                p.LineTo(new Vector2(rect.width, rect.height - i * thickness));
                p.LineTo(new Vector2(rect.width, rect.height - (i + 1) * thickness));
                p.LineTo(new Vector2(0, rect.height - (i + 1) * thickness));
                p.ClosePath();
                p.Fill();
            }
        }
    }
}
