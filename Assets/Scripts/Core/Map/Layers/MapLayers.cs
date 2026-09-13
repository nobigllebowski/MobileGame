using System;
using System.Collections.Generic;
using Nation.Core.Models;

namespace Nation.Core.Map.Layers
{
    public enum MapLayerKind
    {
        Political,
        Economy,
        Population,
        Resources,
        Diplomacy,
        Stability,
        Influence
    }

    public enum MapLayerAvailability
    {
        /// <summary>Every country on the map has a value.</summary>
        Available,
        /// <summary>Only simulated countries have a value; the rest render neutral.</summary>
        Partial,
        /// <summary>The system behind the layer does not exist yet.</summary>
        Unavailable
    }

    public enum MapLayerStyle
    {
        Categorical,
        Gradient
    }

    /// <summary>
    /// Supplies a normalized 0..1 value per country for one thematic layer. Colors are chosen by the renderer,
    /// so the same provider works for the map, a legend and a future minimap.
    /// </summary>
    public interface IMapLayerProvider
    {
        MapLayerKind Kind { get; }
        MapLayerStyle Style { get; }
        MapLayerAvailability Availability { get; }
        string NameKey { get; }
        string DescriptionKey { get; }
        string LegendLowKey { get; }
        string LegendHighKey { get; }
        bool TryGetValue(MapCountry country, out float normalized);
    }

    public static class MapLayerKeys
    {
        public static string Name(MapLayerKind kind) => "layer." + kind.ToString().ToLowerInvariant();
        public static string Description(MapLayerKind kind) => "layer." + kind.ToString().ToLowerInvariant() + ".description";
    }

    /// <summary>Political ownership: 1 = player, 0.5 = simulated nation, 0.25 = dependency, 0 = other.</summary>
    public sealed class PoliticalLayer : IMapLayerProvider
    {
        private readonly Func<WorldState> _world;

        public PoliticalLayer(Func<WorldState> world)
        {
            _world = world;
        }

        public MapLayerKind Kind => MapLayerKind.Political;
        public MapLayerStyle Style => MapLayerStyle.Categorical;
        public MapLayerAvailability Availability => MapLayerAvailability.Available;
        public string NameKey => MapLayerKeys.Name(Kind);
        public string DescriptionKey => MapLayerKeys.Description(Kind);
        public string LegendLowKey => "layer.political.legend_low";
        public string LegendHighKey => "layer.political.legend_high";

        public bool TryGetValue(MapCountry country, out float normalized)
        {
            var world = _world();
            if (world != null && country.Id == world.PlayerCountryId)
            {
                normalized = 1f;
            }
            else if (world != null && world.TryGetCountry(country.Id, out _))
            {
                normalized = 0.5f;
            }
            else if (country.Type == MapTerritoryType.Dependency)
            {
                normalized = 0.25f;
            }
            else
            {
                normalized = 0f;
            }

            return true;
        }
    }

    /// <summary>Base for gradient layers that read live state where simulated and Natural Earth estimates elsewhere.</summary>
    public abstract class GradientLayer : IMapLayerProvider
    {
        protected Func<WorldState> World { get; }

        protected GradientLayer(Func<WorldState> world)
        {
            World = world;
        }

        public abstract MapLayerKind Kind { get; }
        public MapLayerStyle Style => MapLayerStyle.Gradient;
        public abstract MapLayerAvailability Availability { get; }
        public string NameKey => MapLayerKeys.Name(Kind);
        public string DescriptionKey => MapLayerKeys.Description(Kind);
        public abstract string LegendLowKey { get; }
        public abstract string LegendHighKey { get; }
        public abstract bool TryGetValue(MapCountry country, out float normalized);

        protected bool TryGetState(MapCountry country, out CountryState state)
        {
            var world = World();
            if (world != null && world.TryGetCountry(country.Id, out state))
            {
                return true;
            }

            state = null;
            return false;
        }

        /// <summary>Logarithmic normalization between two positive anchors.</summary>
        protected static float LogNormalize(double value, double low, double high)
        {
            if (value <= low) return 0f;
            if (value >= high) return 1f;
            return (float)(Math.Log(value / low) / Math.Log(high / low));
        }

        protected static float Clamp01(float value) => value < 0 ? 0 : value > 1 ? 1 : value;
    }

    /// <summary>Economic strength by annual GDP, log scale from $1B to $30T.</summary>
    public sealed class EconomyLayer : GradientLayer
    {
        public EconomyLayer(Func<WorldState> world) : base(world) { }
        public override MapLayerKind Kind => MapLayerKind.Economy;
        public override MapLayerAvailability Availability => MapLayerAvailability.Available;
        public override string LegendLowKey => "layer.economy.legend_low";
        public override string LegendHighKey => "layer.economy.legend_high";

        public override bool TryGetValue(MapCountry country, out float normalized)
        {
            var gdp = TryGetState(country, out var state) ? state.Gdp : country.GdpEstimate;
            if (gdp <= 0)
            {
                normalized = 0;
                return false;
            }

            normalized = LogNormalize(gdp, 1e9, 3e13);
            return true;
        }
    }

    /// <summary>Population, log scale from 100k to 1.5B.</summary>
    public sealed class PopulationLayer : GradientLayer
    {
        public PopulationLayer(Func<WorldState> world) : base(world) { }
        public override MapLayerKind Kind => MapLayerKind.Population;
        public override MapLayerAvailability Availability => MapLayerAvailability.Available;
        public override string LegendLowKey => "layer.population.legend_low";
        public override string LegendHighKey => "layer.population.legend_high";

        public override bool TryGetValue(MapCountry country, out float normalized)
        {
            var population = TryGetState(country, out var state) ? state.Population : country.PopulationEstimate;
            if (population <= 0)
            {
                normalized = 0;
                return false;
            }

            normalized = LogNormalize(population, 1e5, 1.5e9);
            return true;
        }
    }

    /// <summary>Stability index, simulated countries only.</summary>
    public sealed class StabilityLayer : GradientLayer
    {
        public StabilityLayer(Func<WorldState> world) : base(world) { }
        public override MapLayerKind Kind => MapLayerKind.Stability;
        public override MapLayerAvailability Availability => MapLayerAvailability.Partial;
        public override string LegendLowKey => "layer.stability.legend_low";
        public override string LegendHighKey => "layer.stability.legend_high";

        public override bool TryGetValue(MapCountry country, out float normalized)
        {
            if (TryGetState(country, out var state))
            {
                normalized = Clamp01(state.Stability / 100f);
                return true;
            }

            normalized = 0;
            return false;
        }
    }

    /// <summary>Global influence index, simulated countries only.</summary>
    public sealed class InfluenceLayer : GradientLayer
    {
        public InfluenceLayer(Func<WorldState> world) : base(world) { }
        public override MapLayerKind Kind => MapLayerKind.Influence;
        public override MapLayerAvailability Availability => MapLayerAvailability.Partial;
        public override string LegendLowKey => "layer.influence.legend_low";
        public override string LegendHighKey => "layer.influence.legend_high";

        public override bool TryGetValue(MapCountry country, out float normalized)
        {
            if (TryGetState(country, out var state))
            {
                normalized = Clamp01(state.Influence / 100f);
                return true;
            }

            normalized = 0;
            return false;
        }
    }

    /// <summary>Placeholder for layers whose underlying system arrives in a later phase. Never returns a value.</summary>
    public sealed class UnavailableLayer : IMapLayerProvider
    {
        public UnavailableLayer(MapLayerKind kind)
        {
            Kind = kind;
        }

        public MapLayerKind Kind { get; }
        public MapLayerStyle Style => MapLayerStyle.Gradient;
        public MapLayerAvailability Availability => MapLayerAvailability.Unavailable;
        public string NameKey => MapLayerKeys.Name(Kind);
        public string DescriptionKey => MapLayerKeys.Description(Kind);
        public string LegendLowKey => "layer.unavailable";
        public string LegendHighKey => "layer.unavailable";

        public bool TryGetValue(MapCountry country, out float normalized)
        {
            normalized = 0;
            return false;
        }
    }

    /// <summary>The ordered set of layers the map offers.</summary>
    public sealed class MapLayerSet
    {
        private readonly List<IMapLayerProvider> _layers = new List<IMapLayerProvider>();

        public IReadOnlyList<IMapLayerProvider> Layers => _layers;

        public MapLayerSet(Func<WorldState> world)
        {
            _layers.Add(new PoliticalLayer(world));
            _layers.Add(new EconomyLayer(world));
            _layers.Add(new PopulationLayer(world));
            _layers.Add(new UnavailableLayer(MapLayerKind.Resources));
            _layers.Add(new UnavailableLayer(MapLayerKind.Diplomacy));
            _layers.Add(new StabilityLayer(world));
            _layers.Add(new InfluenceLayer(world));
        }

        public IMapLayerProvider Get(MapLayerKind kind)
        {
            foreach (var layer in _layers)
            {
                if (layer.Kind == kind)
                {
                    return layer;
                }
            }

            throw new ArgumentOutOfRangeException(nameof(kind));
        }
    }
}
