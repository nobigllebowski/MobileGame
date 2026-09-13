using System;
using System.Collections.Generic;
using System.Globalization;

namespace Nation.Core.Localization
{
    /// <summary>
    /// Dictionary-backed localization. Tables are added per locale by the platform layer (JSON, later a
    /// downloaded pack). Missing keys render as "[key]" so gaps are visible during play testing.
    /// </summary>
    public sealed class StringTableLocalizationService : ILocalizationService
    {
        public const string DefaultFallbackLocale = "en";

        private readonly Dictionary<string, Dictionary<string, string>> _tables =
            new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        private readonly string _fallbackLocale;

        public string CurrentLocale { get; private set; }

        /// <summary>Raised the first time a key is requested and not found in any table. Used for logging.</summary>
        public event Action<string> MissingKey;

        private readonly HashSet<string> _reportedMissing = new HashSet<string>(StringComparer.Ordinal);

        public StringTableLocalizationService(string fallbackLocale = DefaultFallbackLocale)
        {
            _fallbackLocale = string.IsNullOrEmpty(fallbackLocale) ? DefaultFallbackLocale : fallbackLocale;
            CurrentLocale = _fallbackLocale;
        }

        public IEnumerable<string> AvailableLocales => _tables.Keys;

        public void AddTable(string locale, IEnumerable<KeyValuePair<string, string>> entries)
        {
            if (string.IsNullOrEmpty(locale))
            {
                throw new ArgumentException("Locale must not be empty.", nameof(locale));
            }

            if (entries == null)
            {
                throw new ArgumentNullException(nameof(entries));
            }

            if (!_tables.TryGetValue(locale, out var table))
            {
                table = new Dictionary<string, string>(StringComparer.Ordinal);
                _tables.Add(locale, table);
            }

            foreach (var entry in entries)
            {
                if (!string.IsNullOrEmpty(entry.Key))
                {
                    table[entry.Key] = entry.Value ?? string.Empty;
                }
            }
        }

        /// <summary>Switches locale. Returns false, leaving the locale unchanged, when no table exists for it.</summary>
        public bool SetLocale(string locale)
        {
            if (string.IsNullOrEmpty(locale) || !_tables.ContainsKey(locale))
            {
                return false;
            }

            CurrentLocale = locale;
            return true;
        }

        public bool Has(string key)
        {
            return TryLookup(key, out _);
        }

        public string Get(string key)
        {
            if (TryLookup(key, out var value))
            {
                return value;
            }

            ReportMissing(key);
            return "[" + key + "]";
        }

        public string Get(string key, params object[] args)
        {
            var pattern = Get(key);
            if (args == null || args.Length == 0)
            {
                return pattern;
            }

            try
            {
                return string.Format(CultureInfo.InvariantCulture, pattern, args);
            }
            catch (FormatException)
            {
                return pattern;
            }
        }

        private bool TryLookup(string key, out string value)
        {
            value = null;
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            if (_tables.TryGetValue(CurrentLocale, out var current) && current.TryGetValue(key, out value))
            {
                return true;
            }

            if (!string.Equals(CurrentLocale, _fallbackLocale, StringComparison.OrdinalIgnoreCase)
                && _tables.TryGetValue(_fallbackLocale, out var fallback)
                && fallback.TryGetValue(key, out value))
            {
                return true;
            }

            return false;
        }

        private void ReportMissing(string key)
        {
            if (key == null || !_reportedMissing.Add(key))
            {
                return;
            }

            MissingKey?.Invoke(key);
        }
    }
}
