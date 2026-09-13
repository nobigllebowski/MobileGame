using System.Collections.Generic;
using System.Globalization;

namespace Nation.Core.Data
{
    public enum JsonType
    {
        Null,
        Bool,
        Number,
        String,
        Array,
        Object
    }

    /// <summary>
    /// Immutable JSON tree node. Missing members and out-of-range indices resolve to a shared Null node,
    /// so data readers can chain lookups and supply fallbacks without null checks.
    /// </summary>
    public sealed class JsonValue
    {
        public static readonly JsonValue Null = new JsonValue(JsonType.Null);

        private static readonly List<JsonValue> EmptyItems = new List<JsonValue>();
        private static readonly Dictionary<string, JsonValue> EmptyMembers = new Dictionary<string, JsonValue>();

        private readonly List<JsonValue> _items;
        private readonly Dictionary<string, JsonValue> _members;

        public JsonType Type { get; }
        public double NumberValue { get; }
        public string StringValue { get; }
        public bool BoolValue { get; }

        private JsonValue(JsonType type)
        {
            Type = type;
        }

        public JsonValue(double number)
        {
            Type = JsonType.Number;
            NumberValue = number;
        }

        public JsonValue(string text)
        {
            Type = JsonType.String;
            StringValue = text;
        }

        public JsonValue(bool value)
        {
            Type = JsonType.Bool;
            BoolValue = value;
        }

        public JsonValue(List<JsonValue> items)
        {
            Type = JsonType.Array;
            _items = items;
        }

        public JsonValue(Dictionary<string, JsonValue> members)
        {
            Type = JsonType.Object;
            _members = members;
        }

        public bool IsNull => Type == JsonType.Null;
        public bool IsObject => Type == JsonType.Object;
        public bool IsArray => Type == JsonType.Array;

        public int Count => Type == JsonType.Array ? _items.Count : Type == JsonType.Object ? _members.Count : 0;

        public JsonValue this[string key]
        {
            get
            {
                if (Type == JsonType.Object && key != null && _members.TryGetValue(key, out var value))
                {
                    return value;
                }

                return Null;
            }
        }

        public JsonValue this[int index]
        {
            get
            {
                if (Type == JsonType.Array && index >= 0 && index < _items.Count)
                {
                    return _items[index];
                }

                return Null;
            }
        }

        public bool Has(string key) => Type == JsonType.Object && _members.ContainsKey(key);

        public IReadOnlyList<JsonValue> Items => Type == JsonType.Array ? _items : EmptyItems;

        public IEnumerable<KeyValuePair<string, JsonValue>> Members => Type == JsonType.Object ? _members : EmptyMembers;

        public string AsString(string fallback = null) => Type == JsonType.String ? StringValue : fallback;

        public double AsDouble(double fallback = 0)
        {
            if (Type == JsonType.Number)
            {
                return NumberValue;
            }

            if (Type == JsonType.String && double.TryParse(StringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
            {
                return parsed;
            }

            return fallback;
        }

        public float AsFloat(float fallback = 0) => (float)AsDouble(fallback);

        public int AsInt(int fallback = 0) => Type == JsonType.Number ? (int)NumberValue : fallback;

        public long AsLong(long fallback = 0) => Type == JsonType.Number ? (long)NumberValue : fallback;

        public bool AsBool(bool fallback = false) => Type == JsonType.Bool ? BoolValue : fallback;

        public string[] AsStringArray()
        {
            if (Type != JsonType.Array)
            {
                return new string[0];
            }

            var result = new string[_items.Count];
            for (var i = 0; i < _items.Count; i++)
            {
                result[i] = _items[i].AsString(string.Empty);
            }

            return result;
        }
    }
}
