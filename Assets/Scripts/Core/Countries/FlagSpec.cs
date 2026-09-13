using System.Collections.Generic;

namespace Nation.Core.Countries
{
    public enum FlagLayerKind
    {
        Fill,
        HorizontalStripes,
        VerticalStripes,
        Disc,
        Ring,
        Star,
        Canton,
        Cross,
        Saltire,
        Diamond
    }

    /// <summary>
    /// One drawing instruction of a vector flag. Positions and sizes are fractions of the flag width and height,
    /// so the same description renders at any resolution. Real flag textures can replace this later; the
    /// definition keeps the vector version as a fallback.
    /// </summary>
    public sealed class FlagLayer
    {
        public FlagLayerKind Kind { get; set; }
        public string[] Colors { get; set; } = new string[0];
        public int Count { get; set; }
        public float X { get; set; } = 0.5f;
        public float Y { get; set; } = 0.5f;
        public float Size { get; set; } = 0.25f;
        public float Width { get; set; }
        public float Height { get; set; }
        public float Thickness { get; set; } = 0.1f;
        public int Points { get; set; } = 5;

        public string Color(int index)
        {
            return Colors.Length == 0 ? "#FFFFFF" : Colors[index % Colors.Length];
        }
    }

    public sealed class FlagSpec
    {
        public static readonly FlagSpec Empty = new FlagSpec(new List<FlagLayer>());

        public IReadOnlyList<FlagLayer> Layers { get; }

        public FlagSpec(IReadOnlyList<FlagLayer> layers)
        {
            Layers = layers;
        }
    }
}
