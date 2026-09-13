using Nation.Core.Data;
using NUnit.Framework;

namespace Nation.Tests
{
    public sealed class JsonParserTests
    {
        [Test]
        public void Parses_Nested_Objects_Arrays_And_Scalars()
        {
            var root = JsonParser.Parse("{ \"a\": 1.5, \"b\": [1, 2, 3], \"c\": { \"d\": \"text\", \"e\": true, \"f\": null }, \"g\": -2e3 }");

            Assert.AreEqual(1.5, root["a"].AsDouble());
            Assert.AreEqual(3, root["b"].Count);
            Assert.AreEqual(2, root["b"][1].AsInt());
            Assert.AreEqual("text", root["c"]["d"].AsString());
            Assert.IsTrue(root["c"]["e"].AsBool());
            Assert.IsTrue(root["c"]["f"].IsNull);
            Assert.AreEqual(-2000.0, root["g"].AsDouble());
        }

        [Test]
        public void Missing_Members_Resolve_To_Null_With_Fallbacks()
        {
            var root = JsonParser.Parse("{}");

            Assert.IsTrue(root["missing"].IsNull);
            Assert.IsTrue(root["missing"]["deeper"][3].IsNull);
            Assert.AreEqual(7, root["missing"].AsInt(7));
            Assert.AreEqual("x", root["missing"].AsString("x"));
        }

        [Test]
        public void Handles_Escapes_And_Unicode()
        {
            var root = JsonParser.Parse("{ \"s\": \"line\\nbreak \\\"quoted\\\" \\u00e9\" }");

            Assert.AreEqual("line\nbreak \"quoted\" \u00e9", root["s"].AsString());
        }

        [Test]
        public void Rejects_Trailing_Garbage_And_Bad_Syntax()
        {
            Assert.Throws<JsonParseException>(() => JsonParser.Parse("{} x"));
            Assert.Throws<JsonParseException>(() => JsonParser.Parse("{ \"a\": }"));
            Assert.Throws<JsonParseException>(() => JsonParser.Parse("[1, 2"));
        }
    }
}
