using Nation.Game.UI.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Nation.Game.UI.Components
{
    /// <summary>Compact line or bar chart drawn with Painter2D. Values are normalized to their own range.</summary>
    public sealed class ChartElement : VisualElement
    {
        public enum Mode { Line, Bars }

        private double[] _values = new double[0];
        private Mode _mode = Mode.Line;
        private Color _color = Palette.Accent;

        public ChartElement(Mode mode)
        {
            _mode = mode;
            AddToClassList("chart");
            pickingMode = PickingMode.Ignore;
            generateVisualContent += OnGenerate;
        }

        public void SetValues(double[] values, Color color)
        {
            _values = values ?? new double[0];
            _color = color;
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
            const float padX = 4f;
            const float padY = 6f;
            var w = rect.width - padX * 2;
            var h = rect.height - padY * 2;

            p.strokeColor = Palette.MapGrid;
            p.lineWidth = 1f;
            for (var i = 0; i <= 3; i++)
            {
                var y = padY + h * i / 3f;
                p.BeginPath();
                p.MoveTo(new Vector2(padX, y));
                p.LineTo(new Vector2(padX + w, y));
                p.Stroke();
            }

            if (_values.Length < 2)
            {
                return;
            }

            double min = double.MaxValue, max = double.MinValue;
            foreach (var v in _values)
            {
                if (v < min) min = v;
                if (v > max) max = v;
            }

            if (max - min < 1e-9)
            {
                max = min + 1;
            }

            var range = max - min;
            var padding = range * 0.15;
            min -= padding;
            max += padding;
            range = max - min;

            if (_mode == Mode.Bars)
            {
                var gap = w / _values.Length;
                var barWidth = gap * 0.55f;
                p.fillColor = _color;
                for (var i = 0; i < _values.Length; i++)
                {
                    var t = (float)((_values[i] - min) / range);
                    var x = padX + gap * i + (gap - barWidth) * 0.5f;
                    var top = padY + h * (1f - t);
                    p.BeginPath();
                    p.MoveTo(new Vector2(x, padY + h));
                    p.LineTo(new Vector2(x, top));
                    p.LineTo(new Vector2(x + barWidth, top));
                    p.LineTo(new Vector2(x + barWidth, padY + h));
                    p.ClosePath();
                    p.Fill();
                }

                return;
            }

            p.fillColor = Palette.WithAlpha(_color, 0.12f);
            p.BeginPath();
            p.MoveTo(new Vector2(padX, padY + h));
            for (var i = 0; i < _values.Length; i++)
            {
                p.LineTo(Point(i, min, range, padX, padY, w, h));
            }

            p.LineTo(new Vector2(padX + w, padY + h));
            p.ClosePath();
            p.Fill();

            p.strokeColor = _color;
            p.lineWidth = 2f;
            p.lineJoin = LineJoin.Round;
            p.lineCap = LineCap.Round;
            p.BeginPath();
            for (var i = 0; i < _values.Length; i++)
            {
                var point = Point(i, min, range, padX, padY, w, h);
                if (i == 0) p.MoveTo(point);
                else p.LineTo(point);
            }

            p.Stroke();

            var last = Point(_values.Length - 1, min, range, padX, padY, w, h);
            p.fillColor = _color;
            p.BeginPath();
            p.Arc(last, 3.5f, Angle.Degrees(0), Angle.Degrees(360));
            p.ClosePath();
            p.Fill();
        }

        private Vector2 Point(int index, double min, double range, float padX, float padY, float w, float h)
        {
            var x = padX + w * index / (_values.Length - 1);
            var t = (float)((_values[index] - min) / range);
            return new Vector2(x, padY + h * (1f - t));
        }
    }
}
