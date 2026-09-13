using System;
using System.Collections.Generic;

namespace Nation.Core.Map.Import
{
    /// <summary>
    /// Rules for turning Natural Earth admin-0 attributes into stable game identifiers.
    /// ISO 3166-1 alpha-3 is used wherever Natural Earth provides one (ISO_A3_EH); the handful of territories
    /// without an ISO code (Kosovo, Somaliland, Northern Cyprus, ...) keep Natural Earth's own ADM0_A3 code
    /// and are flagged so the validator and the UI can tell them apart.
    /// </summary>
    public static class NaturalEarthMapping
    {
        public const string MissingValue = "-99";

        /// <summary>Countries with several official capitals: the seat of government the game uses.</summary>
        public static readonly Dictionary<string, string> CapitalOverrides = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "ZAF", "Pretoria" },
            { "BOL", "La Paz" },
            { "CIV", "Yamoussoukro" },
            { "MMR", "Naypyidaw" },
            { "NLD", "Amsterdam" },
            { "CHE", "Bern" },
            { "MYS", "Kuala Lumpur" },
            { "TZA", "Dodoma" },
            { "LKA", "Colombo" }
        };

        /// <summary>Locales the name tables are generated for, with the Natural Earth attribute that holds each.</summary>
        public static readonly KeyValuePair<string, string>[] NameLocales =
        {
            new KeyValuePair<string, string>("en", "NAME_EN"),
            new KeyValuePair<string, string>("ru", "NAME_RU"),
            new KeyValuePair<string, string>("de", "NAME_DE"),
            new KeyValuePair<string, string>("fr", "NAME_FR"),
            new KeyValuePair<string, string>("es", "NAME_ES"),
            new KeyValuePair<string, string>("zh", "NAME_ZH"),
            new KeyValuePair<string, string>("ja", "NAME_JA"),
            new KeyValuePair<string, string>("ar", "NAME_AR")
        };

        public static string ResolveId(GeoFeature feature, out bool isIso)
        {
            return ResolveId(feature, null, out isIso);
        }

        /// <summary>
        /// Resolves the id, skipping ISO codes already taken by another feature (external territories such as the
        /// Australian Indian Ocean Territories carry the parent's ISO code) in favor of the Natural Earth code.
        /// </summary>
        public static string ResolveId(GeoFeature feature, System.Collections.Generic.ICollection<string> taken, out bool isIso)
        {
            var iso = feature.Text("ISO_A3_EH");
            if (!IsValidCode(iso))
            {
                iso = feature.Text("ISO_A3");
            }

            var adm0 = feature.Text("ADM0_A3");
            if (IsValidCode(iso))
            {
                iso = iso.ToUpperInvariant();
                var conflicts = taken != null && taken.Contains(iso);
                if (!conflicts || !IsValidCode(adm0))
                {
                    isIso = true;
                    return iso;
                }
            }

            isIso = false;
            return IsValidCode(adm0) ? adm0.ToUpperInvariant() : string.Empty;
        }

        public static MapTerritoryType ResolveType(GeoFeature feature)
        {
            switch (feature.Text("TYPE"))
            {
                case "Sovereign country": return MapTerritoryType.SovereignCountry;
                case "Country":
                case "Sovereignty": return MapTerritoryType.Country;
                case "Dependency": return MapTerritoryType.Dependency;
                case "Disputed": return MapTerritoryType.Disputed;
                default: return MapTerritoryType.Indeterminate;
            }
        }

        public static bool IsValidCode(string code)
        {
            return !string.IsNullOrEmpty(code) && code != MissingValue && code.Length == 3;
        }

        public static bool IsAdmin0Capital(GeoFeature place)
        {
            return place.Text("FEATURECLA") == "Admin-0 capital";
        }
    }
}
