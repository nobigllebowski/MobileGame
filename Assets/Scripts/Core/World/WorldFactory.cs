using System;
using System.Collections.Generic;
using Nation.Core.Countries;
using Nation.Core.Models;

namespace Nation.Core.World
{
    /// <summary>Creates a fresh world at the game epoch.</summary>
    public static class WorldFactory
    {
        public static WorldState Create(int seed, string playerCountryId, IEnumerable<CountryState> countries)
        {
            if (string.IsNullOrEmpty(playerCountryId))
            {
                throw new ArgumentException("A player country id is required.", nameof(playerCountryId));
            }

            if (countries == null)
            {
                throw new ArgumentNullException(nameof(countries));
            }

            var world = new WorldState
            {
                Seed = seed,
                CurrentDate = GameDate.Epoch,
                TickCount = 0,
                PlayerCountryId = playerCountryId,
                Countries = new List<CountryState>(countries)
            };

            world.RebuildIndex();

            if (!world.TryGetCountry(playerCountryId, out _))
            {
                throw new ArgumentException("Player country '" + playerCountryId + "' is not among the starting countries.", nameof(playerCountryId));
            }

            return world;
        }

        public static WorldState CreateFromDefinitions(int seed, string playerCountryId, IEnumerable<CountryDefinition> definitions)
        {
            if (definitions == null)
            {
                throw new ArgumentNullException(nameof(definitions));
            }

            var states = new List<CountryState>();
            foreach (var definition in definitions)
            {
                states.Add(CountryStateFactory.Create(definition));
            }

            return Create(seed, playerCountryId, states);
        }
    }
}
