using Nation.Game.UI.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Nation.Game.UI.Components
{
    public enum IconKind
    {
        Nation,
        Economy,
        Build,
        World,
        Power,
        Search,
        Close,
        Back,
        ChevronRight,
        Filter,
        Pause,
        Slow,
        Play,
        Fast,
        VeryFast,
        Menu,
        Alert,
        Check,
        Info,
        Pin,
        Globe
    }

    /// <summary>
    /// Vector line icons drawn with Painter2D on a 24 by 24 grid. No sprite atlas, crisp at every scale,
    /// recolored by setting Color. Keep glyphs simple: two-pixel strokes read well at 20 to 28 px.
    /// </summary>
    public sealed class IconElement : VisualElement
    {
        private IconKind _kind;
        private Color _color = Palette.Text;
        private float _strokeWidth = 1.8f;

        public IconKind Kind
        {
            get => _kind;
            set { _kind = value; MarkDirtyRepaint(); }
        }

        public Color Color
        {
            get => _color;
            set { _color = value; MarkDirtyRepaint(); }
        }

        public float StrokeWidth
        {
            get => _strokeWidth;
            set { _strokeWidth = value; MarkDirtyRepaint(); }
        }

        public IconElement(IconKind kind)
        {
            _kind = kind;
            AddToClassList("icon");
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
            var scale = Mathf.Min(rect.width, rect.height) / 24f;
            var offset = new Vector2((rect.width - 24f * scale) * 0.5f, (rect.height - 24f * scale) * 0.5f);
            var g = new Glyph(p, scale, offset, _color, _strokeWidth);

            switch (_kind)
            {
                case IconKind.Nation:
                    g.Path(new[] { 12f, 3f, 20f, 6f, 20f, 12f, 12f, 21f, 4f, 12f, 4f, 6f }, true);
                    g.Line(12, 8, 12, 15);
                    g.Line(8.5f, 11.5f, 15.5f, 11.5f);
                    break;
                case IconKind.Economy:
                    g.Path(new[] { 3f, 17f, 9f, 11f, 13f, 14f, 21f, 6f }, false);
                    g.Path(new[] { 16f, 6f, 21f, 6f, 21f, 11f }, false);
                    g.Line(3, 21, 21, 21);
                    break;
                case IconKind.Build:
                    g.Rect(4, 13, 7, 7);
                    g.Rect(13, 13, 7, 7);
                    g.Rect(8.5f, 4, 7, 7);
                    break;
                case IconKind.World:
                case IconKind.Globe:
                    g.Circle(12, 12, 9);
                    g.Ellipse(12, 12, 4, 9);
                    g.Line(3, 12, 21, 12);
                    g.Line(4.5f, 7.5f, 19.5f, 7.5f);
                    g.Line(4.5f, 16.5f, 19.5f, 16.5f);
                    break;
                case IconKind.Power:
                    g.Path(new[] { 13f, 2f, 5f, 13f, 11f, 13f, 10f, 22f, 19f, 10f, 13f, 10f }, true);
                    break;
                case IconKind.Search:
                    g.Circle(10.5f, 10.5f, 6.5f);
                    g.Line(15.5f, 15.5f, 21f, 21f);
                    break;
                case IconKind.Close:
                    g.Line(6, 6, 18, 18);
                    g.Line(18, 6, 6, 18);
                    break;
                case IconKind.Back:
                    g.Path(new[] { 15f, 5f, 8f, 12f, 15f, 19f }, false);
                    break;
                case IconKind.ChevronRight:
                    g.Path(new[] { 9f, 5f, 16f, 12f, 9f, 19f }, false);
                    break;
                case IconKind.Filter:
                    g.Line(4, 7, 20, 7);
                    g.Line(7, 12, 17, 12);
                    g.Line(10, 17, 14, 17);
                    break;
                case IconKind.Pause:
                    g.FillRect(6, 5, 4, 14);
                    g.FillRect(14, 5, 4, 14);
                    break;
                case IconKind.Slow:
                    g.Path(new[] { 8f, 5f, 17f, 12f, 8f, 19f }, true);
                    break;
                case IconKind.Play:
                    g.FillPath(new[] { 7f, 4.5f, 19f, 12f, 7f, 19.5f });
                    break;
                case IconKind.Fast:
                    g.FillPath(new[] { 3f, 5f, 12f, 12f, 3f, 19f });
                    g.FillPath(new[] { 12f, 5f, 21f, 12f, 12f, 19f });
                    break;
                case IconKind.VeryFast:
                    g.FillPath(new[] { 1.5f, 6f, 8f, 12f, 1.5f, 18f });
                    g.FillPath(new[] { 8.5f, 6f, 15f, 12f, 8.5f, 18f });
                    g.FillPath(new[] { 15.5f, 6f, 22f, 12f, 15.5f, 18f });
                    break;
                case IconKind.Menu:
                    g.Line(4, 7, 20, 7);
                    g.Line(4, 12, 20, 12);
                    g.Line(4, 17, 20, 17);
                    break;
                case IconKind.Alert:
                    g.Path(new[] { 12f, 3f, 22f, 20f, 2f, 20f }, true);
                    g.Line(12, 9, 12, 14);
                    g.Dot(12, 17);
                    break;
                case IconKind.Check:
                    g.Path(new[] { 4f, 12.5f, 9.5f, 18f, 20f, 6.5f }, false);
                    break;
                case IconKind.Info:
                    g.Circle(12, 12, 9);
                    g.Line(12, 11, 12, 17);
                    g.Dot(12, 7.5f);
                    break;
                case IconKind.Pin:
                    g.Path(new[] { 12f, 22f, 6f, 13f, 6f, 9f, 12f, 3f, 18f, 9f, 18f, 13f }, true);
                    g.Circle(12, 9.5f, 2.5f);
                    break;
            }
        }

        private readonly struct Glyph
        {
            private readonly Painter2D _p;
            private readonly float _s;
            private readonly Vector2 _o;

            public Glyph(Painter2D p, float scale, Vector2 offset, Color color, float strokeWidth)
            {
                _p = p;
                _s = scale;
                _o = offset;
                p.strokeColor = color;
                p.fillColor = color;
                p.lineWidth = strokeWidth * scale;
                p.lineCap = LineCap.Round;
                p.lineJoin = LineJoin.Round;
            }

            private Vector2 P(float x, float y) => new Vector2(_o.x + x * _s, _o.y + y * _s);

            public void Line(float x1, float y1, float x2, float y2)
            {
                _p.BeginPath();
                _p.MoveTo(P(x1, y1));
                _p.LineTo(P(x2, y2));
                _p.Stroke();
            }

            public void Path(float[] xy, bool close)
            {
                _p.BeginPath();
                _p.MoveTo(P(xy[0], xy[1]));
                for (var i = 2; i < xy.Length; i += 2)
                {
                    _p.LineTo(P(xy[i], xy[i + 1]));
                }

                if (close)
                {
                    _p.ClosePath();
                }

                _p.Stroke();
            }

            public void FillPath(float[] xy)
            {
                _p.BeginPath();
                _p.MoveTo(P(xy[0], xy[1]));
                for (var i = 2; i < xy.Length; i += 2)
                {
                    _p.LineTo(P(xy[i], xy[i + 1]));
                }

                _p.ClosePath();
                _p.Fill();
            }

            public void Rect(float x, float y, float w, float h)
            {
                Path(new[] { x, y, x + w, y, x + w, y + h, x, y + h }, true);
            }

            public void FillRect(float x, float y, float w, float h)
            {
                FillPath(new[] { x, y, x + w, y, x + w, y + h, x, y + h });
            }

            public void Circle(float cx, float cy, float r)
            {
                _p.BeginPath();
                _p.Arc(P(cx, cy), r * _s, Angle.Degrees(0), Angle.Degrees(360));
                _p.ClosePath();
                _p.Stroke();
            }

            public void Dot(float cx, float cy)
            {
                _p.BeginPath();
                _p.Arc(P(cx, cy), 1.2f * _s, Angle.Degrees(0), Angle.Degrees(360));
                _p.ClosePath();
                _p.Fill();
            }

            public void Ellipse(float cx, float cy, float rx, float ry)
            {
                const float k = 0.5522847f;
                var c = P(cx, cy);
                var ox = rx * _s * k;
                var oy = ry * _s * k;
                var x0 = c.x - rx * _s;
                var x1 = c.x + rx * _s;
                var y0 = c.y - ry * _s;
                var y1 = c.y + ry * _s;
                _p.BeginPath();
                _p.MoveTo(new Vector2(x0, c.y));
                _p.BezierCurveTo(new Vector2(x0, c.y - oy), new Vector2(c.x - ox, y0), new Vector2(c.x, y0));
                _p.BezierCurveTo(new Vector2(c.x + ox, y0), new Vector2(x1, c.y - oy), new Vector2(x1, c.y));
                _p.BezierCurveTo(new Vector2(x1, c.y + oy), new Vector2(c.x + ox, y1), new Vector2(c.x, y1));
                _p.BezierCurveTo(new Vector2(c.x - ox, y1), new Vector2(x0, c.y + oy), new Vector2(x0, c.y));
                _p.ClosePath();
                _p.Stroke();
            }
        }
    }
}
