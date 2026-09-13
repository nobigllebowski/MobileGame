using Nation.Core.Utilities;
using NUnit.Framework;

namespace Nation.Tests
{
    public sealed class NumberFormattingTests
    {
        private static readonly CompactSuffixes English = new CompactSuffixes("K", "M", "B", "T");

        [Test]
        public void Formats_Hud_Values_Compactly()
        {
            Assert.AreEqual("500.0B", NumberFormatting.FormatCompact(500e9, English));
            Assert.AreEqual("4.5T", NumberFormatting.FormatCompact(4.5e12, English));
            Assert.AreEqual("84.0M", NumberFormatting.FormatCompact(84000000, English));
            Assert.AreEqual("12.3K", NumberFormatting.FormatCompact(12345, English));
        }

        [Test]
        public void Small_Values_Have_No_Suffix()
        {
            Assert.AreEqual("999", NumberFormatting.FormatCompact(999, English));
            Assert.AreEqual("0", NumberFormatting.FormatCompact(0, English));
        }

        [Test]
        public void Negative_Values_Keep_Sign()
        {
            Assert.AreEqual("-2.5B", NumberFormatting.FormatCompact(-2.5e9, English));
        }
    }
}
