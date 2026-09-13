using System.Text;
using Nation.Game.Bootstrap;
using UnityEngine;

namespace Nation.Game.Performance
{
    /// <summary>
    /// Lightweight frame-rate and simulation timing sampler. Numbers are accumulated per frame and folded into a
    /// snapshot twice a second; the overlay is off by default and toggled with F3 in the editor or development builds.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PerformanceMonitor : MonoBehaviour
    {
        private const float SampleInterval = 0.5f;

        private int _frames;
        private float _elapsed;
        private float _worstFrame;
        private readonly StringBuilder _builder = new StringBuilder();

        public float Fps { get; private set; }
        public float AverageFrameMs { get; private set; }
        public float WorstFrameMs { get; private set; }
        public bool OverlayVisible { get; private set; }

        private void Update()
        {
            _frames++;
            var dt = UnityEngine.Time.unscaledDeltaTime;
            _elapsed += dt;
            if (dt > _worstFrame)
            {
                _worstFrame = dt;
            }

            if (_elapsed >= SampleInterval)
            {
                Fps = _frames / _elapsed;
                AverageFrameMs = _elapsed / _frames * 1000f;
                WorstFrameMs = _worstFrame * 1000f;
                _frames = 0;
                _elapsed = 0f;
                _worstFrame = 0f;
                if (OverlayVisible)
                {
                    PushOverlay();
                }
            }

            if ((Application.isEditor || Debug.isDebugBuild) && Input.GetKeyDown(KeyCode.F3))
            {
                Toggle();
            }
        }

        public void Toggle()
        {
            OverlayVisible = !OverlayVisible;
            var ui = GameContext.Current != null ? GameContext.Current.UI : null;
            if (ui == null)
            {
                return;
            }

            if (OverlayVisible)
            {
                PushOverlay();
            }
            else
            {
                ui.SetDebugOverlay(null);
            }
        }

        private void PushOverlay()
        {
            var context = GameContext.Current;
            if (context == null || context.UI == null)
            {
                return;
            }

            _builder.Length = 0;
            _builder.Append("FPS ").Append(Fps.ToString("F0")).Append("  ·  ").Append(AverageFrameMs.ToString("F1")).Append(" ms (worst ").Append(WorstFrameMs.ToString("F1")).Append(")\n");
            if (context.Session != null)
            {
                _builder.Append("TICK ").Append(context.Session.Engine.LastTickMilliseconds.ToString("F2")).Append(" ms  ·  day ").Append(context.Session.World.TickCount).Append('\n');
            }

            if (context.Map != null)
            {
                var stats = context.Map.Stats;
                _builder.Append("MAP ").Append(stats.Band).Append("  ·  ").Append(stats.VisibleVertices).Append(" v / ").Append(stats.VisibleTriangles).Append(" t  ·  ").Append(stats.DrawObjects).Append(" objects\n");
                _builder.Append("LABELS ").Append(stats.VisibleLabels).Append("  ·  capitals ").Append(stats.VisibleCapitals).Append("  ·  label pass ").Append(stats.LastLabelUpdateMilliseconds.ToString("F2")).Append(" ms\n");
                _builder.Append("MESH BUILD ").Append(stats.MeshBuildMilliseconds.ToString("F0")).Append(" ms  ·  recolor ").Append(stats.LastRecolorMilliseconds.ToString("F2")).Append(" ms  ·  load ").Append(context.Map.LoadMilliseconds.ToString("F0")).Append(" ms");
            }

            context.UI.SetDebugOverlay(_builder.ToString());
        }
    }
}
