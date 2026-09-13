using System.Collections.Generic;
using Nation.Core.Countries;
using Nation.Core.Data;
using Nation.Core.Localization;
using NUnit.Framework;

namespace Nation.Tests
{
    public sealed class CountryDataTests
    {
        private static readonly string[] RequiredIds = { "DEU", "USA", "CHN", "RUS", "FRA", "GBR", "JPN", "IND", "BRA", "TUR" };

        private static CountryCatalog LoadCatalog()
        {
            return new CountryCatalog(CountryDataParser.Parse(TestData.Countries));
        }

        private static StringTableLocalizationService LoadLocalization()
        {
            var service = new StringTableLocalizationService();
            var en = LocalizationTableParser.Parse(TestData.English);
            var ru = LocalizationTableParser.Parse(TestData.Russian);
            service.AddTable(en.Locale, en.Strings);
            service.AddTable(ru.Locale, ru.Strings);
            service.SetLocale("en");
            return service;
        }

        [Test]
        public void Loads_All_Required_Playable_Countries()
        {
            var catalog = LoadCatalog();

            Assert.AreEqual(10, catalog.Playable.Count);
            foreach (var id in RequiredIds)
            {
                Assert.IsTrue(catalog.TryGet(id, out var definition), "missing " + id);
                Assert.IsTrue(definition.Playable);
                Assert.AreEqual(2, definition.Iso2.Length);
                Assert.IsTrue(definition.Population > 1000000L);
                Assert.IsTrue(definition.Gdp > 1e11);
                Assert.IsTrue(definition.Treasury > 0);
                Assert.IsTrue(definition.AreaKm2 > 0);
                Assert.IsTrue(definition.EnergyConsumption > 0);
                Assert.AreEqual(3, definition.StrengthKeys.Length);
                Assert.AreEqual(3, definition.ChallengeKeys.Length);
                Assert.IsTrue(definition.Flag.Layers.Count > 0);
            }
        }

        [Test]
        public void Germany_Has_Expected_Starting_Values()
        {
            var germany = LoadCatalog().Get("DEU");

            Assert.AreEqual("DE", germany.Iso2);
            Assert.AreEqual(CountryRegion.Europe, germany.Region);
            Assert.AreEqual(GovernmentType.FederalParliamentaryRepublic, germany.Government);
            Assert.AreEqual(84500000L, germany.Population);
            Assert.AreEqual(4.5e12, germany.Gdp);
            Assert.AreEqual(5.0e11, germany.Treasury);
            Assert.IsTrue(germany.EnergySelfSufficiency < 1.0);
            Assert.AreEqual(42f, germany.DefaultTaxes.IncomeTax);
        }

        [Test]
        public void Iso_Lookup_Is_Case_Insensitive_And_Rejects_Unknown()
        {
            var catalog = LoadCatalog();

            Assert.IsTrue(catalog.TryGet("deu", out var lower));
            Assert.AreEqual("DEU", lower.Id);
            Assert.IsFalse(catalog.TryGet("XXX", out _));
            Assert.IsFalse(catalog.TryGet(null, out _));
            Assert.Throws<KeyNotFoundException>(() => catalog.Get("XXX"));
        }

        [Test]
        public void Duplicate_Ids_Are_Rejected()
        {
            var a = new CountryDefinition { Id = "AAA" };
            var b = new CountryDefinition { Id = "AAA" };

            Assert.Throws<System.ArgumentException>(() => new CountryCatalog(new[] { a, b }));
        }

        [Test]
        public void Search_Matches_Name_Iso_Codes_And_Capital()
        {
            var catalog = LoadCatalog();
            var loc = LoadLocalization();

            var byName = CountryQuery.Apply(catalog.Playable, CountryFilter.None, "germ", loc);
            Assert.AreEqual(1, byName.Count);
            Assert.AreEqual("DEU", byName[0].Id);

            var byIso3 = CountryQuery.Apply(catalog.Playable, CountryFilter.None, "jpn", loc);
            Assert.AreEqual(1, byIso3.Count);
            Assert.AreEqual("JPN", byIso3[0].Id);

            var byIso2 = CountryQuery.Apply(catalog.Playable, CountryFilter.None, "BR", loc);
            Assert.IsTrue(byIso2.Exists(c => c.Id == "BRA"));

            var byCapital = CountryQuery.Apply(catalog.Playable, CountryFilter.None, "berlin", loc);
            Assert.AreEqual(1, byCapital.Count);
            Assert.AreEqual("DEU", byCapital[0].Id);

            var none = CountryQuery.Apply(catalog.Playable, CountryFilter.None, "atlantis", loc);
            Assert.AreEqual(0, none.Count);

            var all = CountryQuery.Apply(catalog.Playable, CountryFilter.None, "   ", loc);
            Assert.AreEqual(10, all.Count);
        }

        [Test]
        public void Search_Uses_The_Active_Locale()
        {
            var catalog = LoadCatalog();
            var loc = LoadLocalization();
            loc.SetLocale("ru");

            var result = CountryQuery.Apply(catalog.Playable, CountryFilter.None, "герм", loc);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("DEU", result[0].Id);
        }

        [Test]
        public void Filters_Narrow_The_List()
        {
            var catalog = LoadCatalog();

            var europe = CountryQuery.Apply(catalog.Playable, new CountryFilter { Region = CountryRegion.Europe }, null, null);
            Assert.AreEqual(4, europe.Count);
            Assert.IsTrue(europe.Exists(c => c.Id == "DEU"));
            Assert.IsTrue(europe.Exists(c => c.Id == "RUS"));
            Assert.IsFalse(europe.Exists(c => c.Id == "USA"));

            var superpowers = CountryQuery.Apply(catalog.Playable, new CountryFilter { Economy = EconomyTier.Superpower }, null, null);
            Assert.AreEqual(2, superpowers.Count);

            var huge = CountryQuery.Apply(catalog.Playable, new CountryFilter { Population = PopulationTier.Huge }, null, null);
            Assert.AreEqual(2, huge.Count);

            var combined = CountryQuery.Apply(catalog.Playable, new CountryFilter { Region = CountryRegion.Asia, Technology = TechnologyTier.Advanced }, null, null);
            Assert.AreEqual(1, combined.Count);
            Assert.AreEqual("JPN", combined[0].Id);

            var filter = new CountryFilter { Region = CountryRegion.Asia, Difficulty = CountryDifficulty.Easy };
            Assert.AreEqual(2, filter.ActiveCount);
            Assert.IsFalse(filter.IsEmpty);
            Assert.IsTrue(CountryFilter.None.IsEmpty);
        }

        [Test]
        public void Difficulty_Is_Derived_From_Data_And_Ordered_Sensibly()
        {
            var catalog = LoadCatalog();

            var usa = CountryDifficultyCalculator.Score(catalog.Get("USA"));
            var germany = CountryDifficultyCalculator.Score(catalog.Get("DEU"));
            var india = CountryDifficultyCalculator.Score(catalog.Get("IND"));

            Assert.IsTrue(usa > germany);
            Assert.IsTrue(germany > india);
            Assert.AreEqual(CountryDifficulty.Easy, CountryDifficultyCalculator.Calculate(catalog.Get("USA")));
            Assert.AreEqual(CountryDifficulty.Normal, CountryDifficultyCalculator.Calculate(catalog.Get("DEU")));
            Assert.AreEqual(CountryDifficulty.VeryHard, CountryDifficultyCalculator.Calculate(catalog.Get("IND")));

            var seen = new HashSet<CountryDifficulty>();
            foreach (var country in catalog.Playable)
            {
                seen.Add(CountryDifficultyCalculator.Calculate(country));
            }

            Assert.IsTrue(seen.Count >= 3, "difficulty should spread across bands");
        }

        [Test]
        public void Every_Text_Key_Referenced_By_Country_Data_Exists_In_English()
        {
            var catalog = LoadCatalog();
            var loc = LoadLocalization();

            foreach (var country in catalog.All)
            {
                Assert.IsTrue(loc.Has(country.NameKey), country.NameKey);
                Assert.IsTrue(loc.Has(country.CapitalKey), country.CapitalKey);
                Assert.IsTrue(loc.Has(country.GovernmentKey), country.GovernmentKey);
                Assert.IsTrue(loc.Has(country.RegionKey), country.RegionKey);
                foreach (var key in country.StrengthKeys) Assert.IsTrue(loc.Has(key), key);
                foreach (var key in country.ChallengeKeys) Assert.IsTrue(loc.Has(key), key);
            }
        }

        [Test]
        public void Russian_Covers_The_Selection_Flow_And_Falls_Back_Elsewhere()
        {
            var loc = LoadLocalization();
            loc.SetLocale("ru");

            Assert.AreEqual("ВЫБЕРИТЕ СТРАНУ", loc.Get("select.title"));
            Assert.AreEqual("ГЕРМАНИЯ", loc.Get("country.DEU"));
            Assert.AreEqual("ВОЗГЛАВИТЬ СТРАНУ", loc.Get("preview.start"));
            Assert.AreEqual("PRIMARY METRICS", loc.Get("nation.primary"));
        }
    }
}
