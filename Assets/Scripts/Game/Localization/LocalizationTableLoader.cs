using System;
using System.Collections.Generic;
using Nation.Core.Localization;
using UnityEngine;

namespace Nation.Game.Localization
{
    /// <summary>
    /// Reads a locale JSON table (see Assets/Data/Localization) into the engine-free localization service.
    /// Uses JsonUtility, so the file is an array of key/value entries rather than an object.
    /// </summary>
    public static class LocalizationTableLoader
    {
#pragma warning disable 0649 // Fields are assigned by JsonUtility.
        [Serializable]
        private sealed class TableFile
        {
            public string locale;
            public Entry[] entries;
        }

        [Serializable]
        private sealed class Entry
        {
            public string key;
            public string value;
        }
#pragma warning restore 0649

        /// <summary>Parses the asset and adds it to the service. Returns the locale that was loaded, or null on failure.</summary>
        public static string LoadInto(StringTableLocalizationService service, TextAsset asset)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            if (asset == null)
            {
                Debug.LogError("[Localization] A localization table reference in the catalog is empty.");
                return null;
            }

            var file = JsonUtility.FromJson<TableFile>(asset.text);
            if (file == null || string.IsNullOrEmpty(file.locale))
            {
                Debug.LogError("[Localization] Table '" + asset.name + "' has no locale field.");
                return null;
            }

            var entries = new List<KeyValuePair<string, string>>(file.entries?.Length ?? 0);
            if (file.entries != null)
            {
                foreach (var entry in file.entries)
                {
                    if (entry != null && !string.IsNullOrEmpty(entry.key))
                    {
                        entries.Add(new KeyValuePair<string, string>(entry.key, entry.value));
                    }
                }
            }

            service.AddTable(file.locale, entries);
            return file.locale;
        }
    }
}
