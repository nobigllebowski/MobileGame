using System;
using System.Collections.Generic;
using Nation.Core.Localization;

namespace Nation.Core.Countries
{
    /// <summary>Optional constraints for the country list. A null field means "any".</summary>
    public struct CountryFilter
    {
        public CountryRegion? Region;
        public PopulationTier? Population;
        public EconomyTier? Economy;
        public TechnologyTier? Technology;
        public InfluenceTier? Influence;
        public CountryDifficulty? Difficulty;

        public static CountryFilter None => new CountryFilter();

        public bool IsEmpty => Region == null && Population == null && Economy == null
                               && Technology == null && Influence == null && Difficulty == null;

        public int ActiveCount
        {
            get
            {
                var count = 0;
                if (Region != null) count++;
                if (Population != null) count++;
                if (Economy != null) count++;
                if (Technology != null) count++;
                if (Influence != null) count++;
                if (Difficulty != null) count++;
                return count;
            }
        }

        public bool Matches(CountryDefinition country)
        {
            if (Region != null && country.Region != Region.Value) return false;
            if (Population != null && CountryTiers.Population(country.Population) != Population.Value) return false;
            if (Economy != null && CountryTiers.Economy(country.Gdp) != Economy.Value) return false;
            if (Technology != null && CountryTiers.Technology(country.Technology) != Technology.Value) return false;
            if (Influence != null && CountryTiers.Influence(country.Influence) != Influence.Value) return false;
            if (Difficulty != null && CountryDifficultyCalculator.Calculate(country) != Difficulty.Value) return false;
            return true;
        }
    }

    /// <summary>Search and filter over country definitions. Search matches localized name, ISO codes and capital.</summary>
    public static class CountryQuery
    {
        public static bool MatchesSearch(CountryDefinition country, string query, ILocalizationService localization)
        {
            if (string.IsNullOrEmpty(query))
            {
                return true;
            }

            var needle = query.Trim();
            if (needle.Length == 0)
            {
                return true;
            }

            if (Contains(country.Id, needle) || Contains(country.Iso2, needle))
            {
                return true;
            }

            if (localization == null)
            {
                return false;
            }

            return Contains(localization.Get(country.NameKey), needle) || Contains(localization.Get(country.CapitalKey), needle);
        }

        public static List<CountryDefinition> Apply(IEnumerable<CountryDefinition> countries, CountryFilter filter, string query, ILocalizationService localization)
        {
            var result = new List<CountryDefinition>();
            foreach (var country in countries)
            {
                if (filter.Matches(country) && MatchesSearch(country, query, localization))
                {
                    result.Add(country);
                }
            }

            return result;
        }

        private static bool Contains(string haystack, string needle)
        {
            return !string.IsNullOrEmpty(haystack) && haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
