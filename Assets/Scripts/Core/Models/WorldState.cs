using System;
using System.Collections.Generic;

namespace Nation.Core.Models
{
    /// <summary>
    /// The single root aggregate of everything dynamic in a game. Serialized as-is for saves and,
    /// later, as the network snapshot. Contains no engine types and no static definitions.
    /// </summary>
    public sealed class WorldState
    {
        private Dictionary<string, CountryState> _countryIndex;

        /// <summary>Seed for all deterministic randomness in this world.</summary>
        public int Seed { get; set; }

        public GameDate CurrentDate { get; set; } = GameDate.Epoch;

        /// <summary>Number of simulation ticks (days) processed since the world was created.</summary>
        public long TickCount { get; set; }

        public string PlayerCountryId { get; set; }

        public List<CountryState> Countries { get; set; } = new List<CountryState>();

        public CountryState PlayerCountry => GetCountry(PlayerCountryId);

        public CountryState GetCountry(string countryId)
        {
            if (TryGetCountry(countryId, out var country))
            {
                return country;
            }

            throw new KeyNotFoundException("No country with id '" + countryId + "' exists in this world.");
        }

        public bool TryGetCountry(string countryId, out CountryState country)
        {
            if (_countryIndex == null || _countryIndex.Count != Countries.Count)
            {
                RebuildIndex();
            }

            if (countryId != null && _countryIndex.TryGetValue(countryId, out country))
            {
                return true;
            }

            country = null;
            return false;
        }

        /// <summary>Call after deserialization or after adding or removing countries.</summary>
        public void RebuildIndex()
        {
            _countryIndex = new Dictionary<string, CountryState>(Countries.Count, StringComparer.Ordinal);
            for (var i = 0; i < Countries.Count; i++)
            {
                var country = Countries[i];
                if (string.IsNullOrEmpty(country.Id))
                {
                    throw new InvalidOperationException("A country in the world has no id.");
                }

                _countryIndex[country.Id] = country;
            }
        }
    }
}
