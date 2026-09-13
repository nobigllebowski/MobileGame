using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Nation.Core.Data
{
    public sealed class JsonParseException : Exception
    {
        public int Position { get; }

        public JsonParseException(string message, int position) : base(message + " (at character " + position + ")")
        {
            Position = position;
        }
    }

    /// <summary>
    /// Small, strict, allocation-conscious JSON reader for static game data. Engine-free so the same data
    /// files load identically in the Unity client, in EditMode tests and on a future server.
    /// </summary>
    public static class JsonParser
    {
        public static JsonValue Parse(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            var reader = new Reader(text);
            reader.SkipWhitespace();
            var value = reader.ReadValue();
            reader.SkipWhitespace();
            if (!reader.AtEnd)
            {
                throw new JsonParseException("Unexpected content after the JSON document", reader.Position);
            }

            return value;
        }

        private sealed class Reader
        {
            private readonly string _text;
            private readonly StringBuilder _buffer = new StringBuilder();

            public int Position { get; private set; }

            public Reader(string text)
            {
                _text = text;
            }

            public bool AtEnd => Position >= _text.Length;

            public void SkipWhitespace()
            {
                while (!AtEnd)
                {
                    var c = _text[Position];
                    if (c == ' ' || c == '\t' || c == '\n' || c == '\r')
                    {
                        Position++;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            public JsonValue ReadValue()
            {
                if (AtEnd)
                {
                    throw new JsonParseException("Unexpected end of JSON", Position);
                }

                var c = _text[Position];
                switch (c)
                {
                    case '{':
                        return ReadObject();
                    case '[':
                        return ReadArray();
                    case '"':
                        return new JsonValue(ReadString());
                    case 't':
                        ExpectLiteral("true");
                        return new JsonValue(true);
                    case 'f':
                        ExpectLiteral("false");
                        return new JsonValue(false);
                    case 'n':
                        ExpectLiteral("null");
                        return JsonValue.Null;
                    default:
                        if (c == '-' || (c >= '0' && c <= '9'))
                        {
                            return ReadNumber();
                        }

                        throw new JsonParseException("Unexpected character '" + c + "'", Position);
                }
            }

            private JsonValue ReadObject()
            {
                Position++;
                var members = new Dictionary<string, JsonValue>(StringComparer.Ordinal);
                SkipWhitespace();
                if (Peek() == '}')
                {
                    Position++;
                    return new JsonValue(members);
                }

                while (true)
                {
                    SkipWhitespace();
                    if (Peek() != '"')
                    {
                        throw new JsonParseException("Expected a property name", Position);
                    }

                    var key = ReadString();
                    SkipWhitespace();
                    Expect(':');
                    SkipWhitespace();
                    members[key] = ReadValue();
                    SkipWhitespace();
                    var next = Peek();
                    Position++;
                    if (next == ',')
                    {
                        continue;
                    }

                    if (next == '}')
                    {
                        return new JsonValue(members);
                    }

                    throw new JsonParseException("Expected ',' or '}' in object", Position - 1);
                }
            }

            private JsonValue ReadArray()
            {
                Position++;
                var items = new List<JsonValue>();
                SkipWhitespace();
                if (Peek() == ']')
                {
                    Position++;
                    return new JsonValue(items);
                }

                while (true)
                {
                    SkipWhitespace();
                    items.Add(ReadValue());
                    SkipWhitespace();
                    var next = Peek();
                    Position++;
                    if (next == ',')
                    {
                        continue;
                    }

                    if (next == ']')
                    {
                        return new JsonValue(items);
                    }

                    throw new JsonParseException("Expected ',' or ']' in array", Position - 1);
                }
            }

            private string ReadString()
            {
                Expect('"');
                _buffer.Length = 0;
                while (true)
                {
                    if (AtEnd)
                    {
                        throw new JsonParseException("Unterminated string", Position);
                    }

                    var c = _text[Position++];
                    if (c == '"')
                    {
                        return _buffer.ToString();
                    }

                    if (c != '\\')
                    {
                        _buffer.Append(c);
                        continue;
                    }

                    if (AtEnd)
                    {
                        throw new JsonParseException("Unterminated escape sequence", Position);
                    }

                    var escaped = _text[Position++];
                    switch (escaped)
                    {
                        case '"': _buffer.Append('"'); break;
                        case '\\': _buffer.Append('\\'); break;
                        case '/': _buffer.Append('/'); break;
                        case 'b': _buffer.Append('\b'); break;
                        case 'f': _buffer.Append('\f'); break;
                        case 'n': _buffer.Append('\n'); break;
                        case 'r': _buffer.Append('\r'); break;
                        case 't': _buffer.Append('\t'); break;
                        case 'u':
                            if (Position + 4 > _text.Length)
                            {
                                throw new JsonParseException("Invalid unicode escape", Position);
                            }

                            var hex = _text.Substring(Position, 4);
                            if (!int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var code))
                            {
                                throw new JsonParseException("Invalid unicode escape", Position);
                            }

                            _buffer.Append((char)code);
                            Position += 4;
                            break;
                        default:
                            throw new JsonParseException("Invalid escape '\\" + escaped + "'", Position - 1);
                    }
                }
            }

            private JsonValue ReadNumber()
            {
                var start = Position;
                if (Peek() == '-')
                {
                    Position++;
                }

                while (!AtEnd)
                {
                    var c = _text[Position];
                    if ((c >= '0' && c <= '9') || c == '.' || c == 'e' || c == 'E' || c == '+' || c == '-')
                    {
                        Position++;
                    }
                    else
                    {
                        break;
                    }
                }

                var slice = _text.Substring(start, Position - start);
                if (!double.TryParse(slice, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
                {
                    throw new JsonParseException("Invalid number '" + slice + "'", start);
                }

                return new JsonValue(number);
            }

            private char Peek()
            {
                if (AtEnd)
                {
                    throw new JsonParseException("Unexpected end of JSON", Position);
                }

                return _text[Position];
            }

            private void Expect(char expected)
            {
                if (Peek() != expected)
                {
                    throw new JsonParseException("Expected '" + expected + "'", Position);
                }

                Position++;
            }

            private void ExpectLiteral(string literal)
            {
                if (string.CompareOrdinal(_text, Position, literal, 0, literal.Length) != 0)
                {
                    throw new JsonParseException("Invalid literal", Position);
                }

                Position += literal.Length;
            }
        }
    }
}
