using Nation.Core.Map.Layers;

namespace Nation.Core.Signals
{
    /// <summary>Published when the selected map country changes. CountryId is null when the selection is cleared.</summary>
    public readonly struct CountrySelectedSignal : ISignal
    {
        public string CountryId { get; }
        public bool IsSelection => CountryId != null;

        public CountrySelectedSignal(string countryId)
        {
            CountryId = countryId;
        }
    }

    public readonly struct MapLayerChangedSignal : ISignal
    {
        public MapLayerKind Layer { get; }

        public MapLayerChangedSignal(MapLayerKind layer)
        {
            Layer = layer;
        }
    }
}
