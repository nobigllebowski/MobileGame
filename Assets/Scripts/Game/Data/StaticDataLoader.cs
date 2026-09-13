using System;
using System.Collections.Generic;
using Nation.Core.Buildings;
using Nation.Core.Countries;
using Nation.Core.Data;
using Nation.Core.Localization;
using UnityEngine;

namespace Nation.Game.Data
{
    /// <summary>
    /// Thin bridge from TextAssets to the engine-free parsers. Every failure is logged with the asset name and
    /// replaced by an empty result so the game still reaches the menu and shows what is missing.
    /// </summary>
    public static class StaticDataLoader
    {
        public static ICountryDataProvider LoadCountries(TextAsset asset)
        {
            try
            {
                if (asset == null)
                {
                    throw new InvalidOperationException("no countries asset assigned in the Game Data Catalog");
                }

                var definitions = CountryDataParser.Parse(asset.text);
                Debug.Log("[Data] Loaded " + definitions.Count + " countries from " + asset.name + ".");
                return new CountryCatalog(definitions);
            }
            catch (Exception exception)
            {
                Debug.LogError("[Data] Country data failed to load: " + exception.Message);
                return new CountryCatalog(new List<CountryDefinition>());
            }
        }

        public static IReadOnlyList<BuildingDefinition> LoadBuildings(TextAsset asset)
        {
            try
            {
                if (asset == null)
                {
                    throw new InvalidOperationException("no buildings asset assigned in the Game Data Catalog");
                }

                return BuildingDataParser.Parse(asset.text);
            }
            catch (Exception exception)
            {
                Debug.LogError("[Data] Building data failed to load: " + exception.Message);
                return new List<BuildingDefinition>();
            }
        }

        /// <summary>Loads every table into the service and returns the first locale loaded, or null.</summary>
        public static string LoadLocalization(StringTableLocalizationService service, TextAsset[] tables)
        {
            string first = null;
            if (tables == null)
            {
                return null;
            }

            foreach (var table in tables)
            {
                if (table == null)
                {
                    continue;
                }

                try
                {
                    var parsed = LocalizationTableParser.Parse(table.text);
                    service.AddTable(parsed.Locale, parsed.Strings);
                    if (first == null)
                    {
                        first = parsed.Locale;
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogError("[Localization] Table '" + table.name + "' failed to load: " + exception.Message);
                }
            }

            return first;
        }
    }
}
