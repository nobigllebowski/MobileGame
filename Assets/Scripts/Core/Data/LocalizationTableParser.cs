using System.Collections.Generic;

namespace Nation.Core.Data
{
    /// <summary>Parses a locale file: { "locale": "en", "strings": { "key": "value", ... } }.</summary>
    public static class LocalizationTableParser
    {
        public sealed class Result
        {
            public string Locale { get; }
            public Dictionary<string, string> Strings { get; }

            public Result(string locale, Dictionary<string, string> strings)
            {
                Locale = locale;
                Strings = strings;
            }
        }

        public static Result Parse(string json)
        {
            var root = JsonParser.Parse(json);
            var locale = root["locale"].AsString();
            if (string.IsNullOrEmpty(locale))
            {
                throw new JsonParseException("Localization table has no 'locale' field", 0);
            }

            var strings = new Dictionary<string, string>(System.StringComparer.Ordinal);
            foreach (var member in root["strings"].Members)
            {
                strings[member.Key] = member.Value.AsString(string.Empty);
            }

            return new Result(locale, strings);
        }
    }
}
