using System.Collections.Generic;
using Nation.Core.Models;

namespace Nation.Game.Session
{
    /// <summary>
    /// PHASE 1 PLACEHOLDER. Hardcoded starting state for a single country so the game loop can be exercised
    /// before static country data exists. Phase 3 replaces this with ICountryDataProvider and deletes the file.
    /// </summary>
    internal static class PlaceholderCountries
    {
        public const string DefaultPlayerCountryId = "DEU";

        public static IEnumerable<CountryState> Create()
        {
            yield return new CountryState("DEU")
            {
                Population = 84000000L,
                Gdp = 4.5e12,
                Treasury = 500e9,
                Happiness = 62f,
                Influence = 74f,
                EnergyProduction = 100,
                EnergyConsumption = 108
            };
        }
    }
}
