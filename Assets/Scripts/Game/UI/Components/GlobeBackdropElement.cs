using UnityEngine;
using UnityEngine.UIElements;

namespace Nation.Game.UI.Components
{
    /// <summary>
    /// Vector-drawn menu backdrop: a soft radial glow and a slowly turning wireframe globe.
    /// Drawn with Painter2D so it costs no textures and scales to any resolution. The textured Earth
    /// replaces it in the world map phase.
    /// </summary>
    public sealed class GlobeBackdropElement : VisualElement
    {
        private const int Meridians = 12;
        private const int Parallels = 7;
        private const float DegreesPerSecond = 4f;
        private const long FrameIntervalMs = 40;

        private static readonly Color GlowColor = new Color(0.25f, 0.45f, 0.85f, 0.03f);
        private static readonly Color BodyColor = new Color(0.06f, 0.12f, 0.26f, 0.55f);
        private static readonly Color RimColor = new Color(0.45f, 0.65f, 1f, 0.35f);
        private static readonly Color LineColor = new Color(0.45f, 0.65f, 1f, 0.16f);

        private float _rotationDegrees;
        private IVisualElementScheduledItem _animation;

        public GlobeBackdropElement()
        {
            name = "globe-backdrop";
            AddToClassList("globe-backdrop");
            pickingMode = PickingMode.Ignore;
            generateVisualContent += OnGenerateVisualContent;
            RegisterCallback<AttachToPanelEvent>(_ => StartAnimation());
            RegisterCallback<DetachFromPanelEvent>(_ => StopAnimation());
        }

        private void StartAnimation()
        {
            StopAnimation();
            _animation = schedule.Execute(Advance).Every(FrameIntervalMs);
        }

        private void StopAnimation()
        {
            _animation?.Pause();
            _animation = null;
        }

        private void Advance(TimerState state)
        {
            _rotationDegrees = (_rotationDegrees + DegreesPerSecond * state.deltaTime / 1000f) % 360f;
            MarkDirtyRepaint();
        }

        private void OnGenerateVisualContent(MeshGenerationContext context)
        {
            var rect = contentRect;
            if (rect.width <= 0 || rect.height <= 0)
            {
                return;
            }

            var painter = context.painter2D;
            var center = new Vector2(rect.width * 0.5f, rect.height * 0.46f);
            var radius = rect.width * 0.72f;

            DrawGlow(painter, center, radius * 1.35f);
            DrawGlobe(painter, center, radius);
        }

        private static void DrawGlow(Painter2D painter, Vector2 center, float outerRadius)
        {
            painter.fillColor = GlowColor;
            const int rings = 14;
            for (var i = rings; i >= 1; i--)
            {
                var r = outerRadius * i / rings;
                painter.BeginPath();
                painter.Arc(center, r, Angle.Degrees(0), Angle.Degrees(360));
                painter.ClosePath();
                painter.Fill();
            }
        }

        private void DrawGlobe(Painter2D painter, Vector2 center, float radius)
        {
            painter.fillColor = BodyColor;
            painter.BeginPath();
            painter.Arc(center, radius, Angle.Degrees(0), Angle.Degrees(360));
            painter.ClosePath();
            painter.Fill();

            painter.lineWidth = 1f;
            painter.strokeColor = LineColor;

            for (var i = 0; i < Meridians; i++)
            {
                var longitude = (_rotationDegrees + i * 180f / Meridians) * Mathf.Deg2Rad;
                var halfWidth = Mathf.Abs(Mathf.Sin(longitude)) * radius;
                if (halfWidth < 1f)
                {
                    continue;
                }

                DrawEllipse(painter, center, halfWidth, radius);
            }

            for (var i = 1; i < Parallels; i++)
            {
                var latitude = (-90f + 180f * i / Parallels) * Mathf.Deg2Rad;
                var y = center.y + Mathf.Sin(latitude) * radius;
                var halfWidth = Mathf.Cos(latitude) * radius;
                painter.BeginPath();
                painter.MoveTo(new Vector2(center.x - halfWidth, y));
                painter.LineTo(new Vector2(center.x + halfWidth, y));
                painter.Stroke();
            }

            painter.lineWidth = 1.5f;
            painter.strokeColor = RimColor;
            painter.BeginPath();
            painter.Arc(center, radius, Angle.Degrees(0), Angle.Degrees(360));
            painter.ClosePath();
            painter.Stroke();
        }

        private static void DrawEllipse(Painter2D painter, Vector2 center, float radiusX, float radiusY)
        {
            const float kappa = 0.5522847f;
            var ox = radiusX * kappa;
            var oy = radiusY * kappa;
            var x0 = center.x - radiusX;
            var x1 = center.x + radiusX;
            var y0 = center.y - radiusY;
            var y1 = center.y + radiusY;

            painter.BeginPath();
            painter.MoveTo(new Vector2(x0, center.y));
            painter.BezierCurveTo(new Vector2(x0, center.y - oy), new Vector2(center.x - ox, y0), new Vector2(center.x, y0));
            painter.BezierCurveTo(new Vector2(center.x + ox, y0), new Vector2(x1, center.y - oy), new Vector2(x1, center.y));
            painter.BezierCurveTo(new Vector2(x1, center.y + oy), new Vector2(center.x + ox, y1), new Vector2(center.x, y1));
            painter.BezierCurveTo(new Vector2(center.x - ox, y1), new Vector2(x0, center.y + oy), new Vector2(x0, center.y));
            painter.ClosePath();
            painter.Stroke();
        }
    }
}
