using System.Collections.Generic;
using Nation.Core.Localization;
using NUnit.Framework;

namespace Nation.Tests
{
    public sealed class LocalizationServiceTests
    {
        private static StringTableLocalizationService CreateService()
        {
            var service = new StringTableLocalizationService();
            service.AddTable("en", new Dictionary<string, string>
            {
                { "menu.play", "PLAY" },
                { "date.long", "{0} {1}, {2}" },
                { "only.english", "English only" }
            });
            service.AddTable("de", new Dictionary<string, string>
            {
                { "menu.play", "SPIELEN" }
            });
            return service;
        }

        [Test]
        public void Returns_Value_For_Current_Locale()
        {
            var service = CreateService();

            Assert.AreEqual("en", service.CurrentLocale);
            Assert.AreEqual("PLAY", service.Get("menu.play"));

            Assert.IsTrue(service.SetLocale("de"));
            Assert.AreEqual("SPIELEN", service.Get("menu.play"));
        }

        [Test]
        public void Falls_Back_To_English_When_Key_Missing_In_Locale()
        {
            var service = CreateService();
            service.SetLocale("de");

            Assert.AreEqual("English only", service.Get("only.english"));
        }

        [Test]
        public void Missing_Key_Renders_As_Bracketed_Key_And_Reports_Once()
        {
            var service = CreateService();
            var reported = new List<string>();
            service.MissingKey += reported.Add;

            Assert.AreEqual("[does.not.exist]", service.Get("does.not.exist"));
            service.Get("does.not.exist");

            Assert.IsFalse(service.Has("does.not.exist"));
            Assert.AreEqual(1, reported.Count);
        }

        [Test]
        public void Format_Arguments_Are_Applied()
        {
            var service = CreateService();

            Assert.AreEqual("January 2, 2026", service.Get("date.long", "January", 2, 2026));
        }

        [Test]
        public void Unknown_Locale_Is_Rejected()
        {
            var service = CreateService();

            Assert.IsFalse(service.SetLocale("xx"));
            Assert.AreEqual("en", service.CurrentLocale);
        }
    }
}
