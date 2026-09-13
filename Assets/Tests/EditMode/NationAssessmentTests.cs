using Nation.Core.Countries;
using Nation.Core.Data;
using Nation.Core.Economy;
using Nation.Core.Nation;
using Nation.Core.Power;
using Nation.Core.World;
using NUnit.Framework;

namespace Nation.Tests
{
    public sealed class NationAssessmentTests
    {
        private static CountryCatalog Catalog() => new CountryCatalog(CountryDataParser.Parse(TestData.Countries));

        [Test]
        public void Germany_Reports_Energy_Warning_And_Aging_Alert()
        {
            var catalog = Catalog();
            var world = WorldFactory.CreateFromDefinitions(3, "DEU", catalog.Playable);
            var state = world.PlayerCountry;

            var status = NationAssessment.Status(state);
            Assert.AreEqual(4, status.Count);
            Assert.AreEqual("status.strong", status[0].ValueKey);
            Assert.AreEqual(StatusLevel.Danger, status[1].Level);

            var alerts = NationAssessment.Alerts(state, catalog.Get("DEU"));
            Assert.IsTrue(alerts.Exists(a => a.TitleKey == "alert.energy_dependency.title"));
            Assert.IsTrue(alerts.Exists(a => a.TitleKey == "alert.population_growth.title"));
            Assert.IsTrue(alerts.Exists(a => a.TitleKey == "alert.trade_opportunities.title"));
        }

        [Test]
        public void Economy_Overview_Is_Coherent_And_Deterministic()
        {
            var world = WorldFactory.CreateFromDefinitions(11, "DEU", Catalog().Playable);

            var a = EconomyOverview.From(world.PlayerCountry, world.Seed);
            var b = EconomyOverview.From(world.PlayerCountry, world.Seed);

            Assert.AreEqual(a.AnnualRevenue, b.AnnualRevenue);
            Assert.AreEqual(12, a.GdpHistory.Length);
            Assert.AreEqual(a.Gdp, a.GdpHistory[11]);
            Assert.IsTrue(a.AnnualRevenue > 1e12 && a.AnnualRevenue < 3e12);
            Assert.IsTrue(a.GdpGrowth > -3 && a.GdpGrowth < 8);
        }

        [Test]
        public void Power_Overview_Ranks_Largest_Economy_At_Full_Economic_Power()
        {
            var world = WorldFactory.CreateFromDefinitions(1, "USA", Catalog().Playable);

            var usa = PowerOverview.From(world.PlayerCountry, world.Countries);
            var turkey = PowerOverview.From(world.GetCountry("TUR"), world.Countries);

            Assert.AreEqual(100f, usa.EconomicPower);
            Assert.IsTrue(usa.NationalPower > turkey.NationalPower);
            Assert.IsTrue(turkey.EconomicPower < 10f);
        }
    }
}
