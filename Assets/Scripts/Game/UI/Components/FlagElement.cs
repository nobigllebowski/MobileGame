using Nation.Core.Countries;
using Nation.Game.UI.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Nation.Game.UI.Components
{
    /// <summary>
    /// Draws a country flag from its vector FlagSpec. Fractions in the spec map to the element's content box,
    /// so one description works for the 34 px list flag and the 120 px preview flag alike.
    /// </summary>
    public sealed class FlagElement : VisualElement
    {
        private FlagSpec _spec = FlagSpec.Empty;

        public FlagElement()
        {
            AddToClassList("flag");
            pickingMode = PickingMode.Ignore;
            generateVisualContent += OnGenerate;
        }

        public FlagElement(FlagSpec spec, string sizeClass = null) : this()
        {
            _spec = spec ?? FlagSpec.Empty;
            if (!string.IsNullOrEmpty(sizeClass))
            {
                AddToClassList(sizeClass);
            }
        }

        public void SetFlag(FlagSpec spec)
        {
            _spec = spec ?? FlagSpec.Empty;
            MarkDirtyRepaint();
        }

        private void OnGenerate(MeshGenerationContext context)
        {
            var rect = contentRect;
            if (rect.width <= 0 || rect.height <= 0)
            {
                return;
            }

            var p = context.painter2D;
            var w = rect.width;
            var h = rect.height;

            if (_spec.Layers.Count == 0)
            {
                FillRect(p, 0, 0, w, h, Palette.Surface);
                return;
            }

            foreach (var layer in _spec.Layers)
            {
                switch (layer.Kind)
                {
                    case FlagLayerKind.Fill:
                        FillRect(p, 0, 0, w, h, Palette.Hex(layer.Color(0)));
                        break;
                    case FlagLayerKind.HorizontalStripes:
                    {
                        var count = layer.Count > 0 ? layer.Count : layer.Colors.Length;
                        var stripe = h / count;
                        for (var i = 0; i < count; i++)
                        {
                            FillRect(p, 0, i * stripe, w, stripe + 0.5f, Palette.Hex(layer.Color(i)));
                        }

                        break;
                    }
                    case FlagLayerKind.VerticalStripes:
                    {
                        var count = layer.Count > 0 ? layer.Count : layer.Colors.Length;
                        var stripe = w / count;
                        for (var i = 0; i < count; i++)
                        {
                            FillRect(p, i * stripe, 0, stripe + 0.5f, h, Palette.Hex(layer.Color(i)));
                        }

                        break;
                    }
                    case FlagLayerKind.Disc:
                        p.fillColor = Palette.Hex(layer.Color(0));
                        p.BeginPath();
                        p.Arc(new Vector2(layer.X * w, layer.Y * h), layer.Size * h, Angle.Degrees(0), Angle.Degrees(360));
                        p.ClosePath();
                        p.Fill();
                        break;
                    case FlagLayerKind.Ring:
                        p.strokeColor = Palette.Hex(layer.Color(0));
                        p.lineWidth = Mathf.Max(1f, layer.Thickness * h);
                        p.BeginPath();
                        p.Arc(new Vector2(layer.X * w, layer.Y * h), layer.Size * h, Angle.Degrees(0), Angle.Degrees(360));
                        p.ClosePath();
                        p.Stroke();
                        break;
                    case FlagLayerKind.Star:
                        Star(p, new Vector2(layer.X * w, layer.Y * h), layer.Size * h, layer.Points, Palette.Hex(layer.Color(0)));
                        break;
                    case FlagLayerKind.Canton:
                        FillRect(p, 0, 0, layer.Width * w, layer.Height * h, Palette.Hex(layer.Color(0)));
                        break;
                    case FlagLayerKind.Cross:
                    {
                        var t = layer.Thickness * h;
                        var color = Palette.Hex(layer.Color(0));
                        FillRect(p, 0, h * 0.5f - t * 0.5f, w, t, color);
                        FillRect(p, w * 0.5f - t * 0.5f, 0, t, h, color);
                        break;
                    }
                    case FlagLayerKind.Saltire:
                    {
                        var t = layer.Thickness * h;
                        var color = Palette.Hex(layer.Color(0));
                        Band(p, new Vector2(0, 0), new Vector2(w, h), t, color);
                        Band(p, new Vector2(w, 0), new Vector2(0, h), t, color);
                        break;
                    }
                    case FlagLayerKind.Diamond:
                    {
                        var hw = layer.Width * w * 0.5f;
                        var hh = layer.Height * h * 0.5f;
                        var c = new Vector2(w * 0.5f, h * 0.5f);
                        p.fillColor = Palette.Hex(layer.Color(0));
                        p.BeginPath();
                        p.MoveTo(new Vector2(c.x, c.y - hh));
                        p.LineTo(new Vector2(c.x + hw, c.y));
                        p.LineTo(new Vector2(c.x, c.y + hh));
                        p.LineTo(new Vector2(c.x - hw, c.y));
                        p.ClosePath();
                        p.Fill();
                        break;
                    }
                }
            }
        }

        private static void FillRect(Painter2D p, float x, float y, float w, float h, Color color)
        {
            p.fillColor = color;
            p.BeginPath();
            p.MoveTo(new Vector2(x, y));
            p.LineTo(new Vector2(x + w, y));
            p.LineTo(new Vector2(x + w, y + h));
            p.LineTo(new Vector2(x, y + h));
            p.ClosePath();
            p.Fill();
        }

        private static void Band(Painter2D p, Vector2 from, Vector2 to, float thickness, Color color)
        {
            var dir = (to - from).normalized;
            var normal = new Vector2(-dir.y, dir.x) * (thickness * 0.5f);
            p.fillColor = color;
            p.BeginPath();
            p.MoveTo(from + normal);
            p.LineTo(to + normal);
            p.LineTo(to - normal);
            p.LineTo(from - normal);
            p.ClosePath();
            p.Fill();
        }

        private static void Star(Painter2D p, Vector2 center, float outer, int points, Color color)
        {
            if (points < 3)
            {
                points = 5;
            }

            var inner = outer * 0.382f;
            p.fillColor = color;
            p.BeginPath();
            for (var i = 0; i < points * 2; i++)
            {
                var r = (i % 2 == 0) ? outer : inner;
                var angle = -Mathf.PI / 2f + i * Mathf.PI / points;
                var point = new Vector2(center.x + Mathf.Cos(angle) * r, center.y + Mathf.Sin(angle) * r);
                if (i == 0) p.MoveTo(point);
                else p.LineTo(point);
            }

            p.ClosePath();
            p.Fill();
        }
    }
}
