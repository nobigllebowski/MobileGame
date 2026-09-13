using Nation.Core.Map;

namespace Nation.Game.Map
{
    /// <summary>Counters the map systems update so the performance overlay can report them without polling them.</summary>
    public sealed class MapPerformanceStats
    {
        public MapZoomBand Band;
        public int VisibleVertices;
        public int VisibleTriangles;
        public int DrawObjects;
        public int VisibleLabels;
        public int VisibleCapitals;
        public double MeshBuildMilliseconds;
        public double LastRecolorMilliseconds;
        public double LastLabelUpdateMilliseconds;
        public int CameraUpdatesThisSecond;
        public int LabelUpdatesThisSecond;
    }
}
