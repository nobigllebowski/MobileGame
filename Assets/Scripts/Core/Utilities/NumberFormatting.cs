using System;
using System.Globalization;

namespace Nation.Core.Utilities
{
    /// <summary>Localized magnitude suffixes, for example K, M, B, T in English.</summary>
    public readonly struct CompactSuffixes
    {
        public string Thousand { get; }
        public string Million { get; }
        public string Billion { get; }
        public string Trillion { get; }

        public CompactSuffixes(string thousand, string million, string billion, string trillion)
        {
            Thousand = thousand;
            Million = million;
            Billion = billion;
            Trillion = trillion;
        }
    }

    /// <summary>
    /// Compact number formatting for HUD values such as "$4.5T" or "84.0M".
    /// Uses the invariant culture; locale-specific separators arrive with the localization phase.
    /// </summary>
    public static class NumberFormatting
    {
        public static string FormatCompact(double value, CompactSuffixes suffixes, int decimals = 1)
        {
            var magnitude = Math.Abs(value);
            double scaled;
            string suffix;

            if (magnitude >= 1e12)
            {
                scaled = value / 1e12;
                suffix = suffixes.Trillion;
            }
            else if (magnitude >= 1e9)
            {
                scaled = value / 1e9;
                suffix = suffixes.Billion;
            }
            else if (magnitude >= 1e6)
            {
                scaled = value / 1e6;
                suffix = suffixes.Million;
            }
            else if (magnitude >= 1e3)
            {
                scaled = value / 1e3;
                suffix = suffixes.Thousand;
            }
            else
            {
                return value.ToString("F0", CultureInfo.InvariantCulture);
            }

            return scaled.ToString("F" + decimals, CultureInfo.InvariantCulture) + suffix;
        }
    }
}
