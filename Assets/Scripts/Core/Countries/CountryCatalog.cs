using System;
using System.Collections.Generic;

namespace Nation.Core.Countries
{
    /// <summary>In-memory ICountryDataProvider over a parsed list of definitions, indexed by ISO3.</summary>
    public sealed class CountryCatalog : ICountryDataProvider
    {
        private readonly List<CountryDefinition> _all;
        private readonly List<CountryDefinition> _playable;
        private readonly Dictionary<string, CountryDefinition> _byId;

        public IReadOnlyList<CountryDefinition> All => _all;
        public IReadOnlyList<CountryDefinition> Playable => _playable;

        public CountryCatalog(IEnumerable<CountryDefinition> definitions)
        {
            if (definitions == null)
            {
                throw new ArgumentNullException(nameof(definitions));
            }

            _all = new List<CountryDefinition>(definitions);
            _playable = new List<CountryDefinition>();
            _byId = new Dictionary<string, CountryDefinition>(StringComparer.OrdinalIgnoreCase);

            foreach (var definition in _all)
            {
                if (string.IsNullOrEmpty(definition.Id) || definition.Id.Length != 3)
                {
                    throw new ArgumentException("Every country needs a three-letter ISO3 id. Offending entry: '" + definition.Id + "'.");
                }

                if (_byId.ContainsKey(definition.Id))
                {
                    throw new ArgumentException("Duplicate country id '" + definition.Id + "'.");
                }

                _byId.Add(definition.Id, definition);
                if (definition.Playable)
                {
                    _playable.Add(definition);
                }
            }
        }

        public bool TryGet(string iso3, out CountryDefinition definition)
        {
            if (iso3 == null)
            {
                definition = null;
                return false;
            }

            return _byId.TryGetValue(iso3, out definition);
        }

        public CountryDefinition Get(string iso3)
        {
            if (TryGet(iso3, out var definition))
            {
                return definition;
            }

            throw new KeyNotFoundException("Unknown country '" + iso3 + "'.");
        }
    }
}
