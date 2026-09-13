using System.Collections.Generic;
using Nation.Core.Models;
using Nation.Core.Power;
using Nation.Game.Bootstrap;
using Nation.Game.UI.Components;
using Nation.Game.UI.Screens;
using UnityEngine.UIElements;

namespace Nation.Game.UI.Tabs
{
    /// <summary>Strategic standing: composite power, four component ratings and world rankings.</summary>
    public sealed class PowerTab : GameTab
    {
        private Label _nationalValue;
        private GaugeBar _national;
        private GaugeBar _military;
        private GaugeBar _economic;
        private GaugeBar _technological;
        private GaugeBar _influence;
        private Label _economyRank;
        private Label _militaryRank;
        private Label _influenceRank;

        public PowerTab(GameContext context, GameShellScreen shell) : base(context, shell, "tab-power")
        {
        }

        protected override void Build()
        {
            var scroll = Scroll("power");
            Add(scroll);

            scroll.Add(TabHeader(Loc.Get("power.title"), Loc.Get("power.subtitle")));

            var hero = new VisualElement();
            hero.AddToClassList("card");
            hero.AddToClassList("power-hero");
            hero.Add(Typography.Caption(Loc.Get("power.national"), "power-hero__label"));
            _nationalValue = Typography.Display(string.Empty, "power-hero__value");
            hero.Add(_nationalValue);
            _national = new GaugeBar(Loc.Get("power.index"), string.Empty, 0f, "gauge--gold");
            hero.Add(_national);
            scroll.Add(hero);

            var ratings = new InfoCard(Loc.Get("power.ratings"));
            _military = new GaugeBar(Loc.Get("power.military"), string.Empty, 0f);
            _economic = new GaugeBar(Loc.Get("power.economic"), string.Empty, 0f);
            _technological = new GaugeBar(Loc.Get("power.technological"), string.Empty, 0f);
            _influence = new GaugeBar(Loc.Get("power.influence"), string.Empty, 0f);
            ratings.Content.Add(_military);
            ratings.Content.Add(_economic);
            ratings.Content.Add(_technological);
            ratings.Content.Add(_influence);
            scroll.Add(ratings);

            var position = new InfoCard(Loc.Get("power.position"), Loc.Get("power.position_hint"));
            _economyRank = position.AddRow(Loc.Get("power.rank_economy"), string.Empty);
            _militaryRank = position.AddRow(Loc.Get("power.rank_military"), string.Empty);
            _influenceRank = position.AddRow(Loc.Get("power.rank_influence"), string.Empty);
            scroll.Add(position);

            var spacer = new VisualElement();
            spacer.AddToClassList("tab__spacer");
            scroll.Add(spacer);

            Refresh();
        }

        public override void OnShow()
        {
            if (IsBuilt)
            {
                Refresh();
            }
        }

        public override void OnTick()
        {
            Refresh();
        }

        private void Refresh()
        {
            var world = Session.World.Countries;
            var country = Session.PlayerCountry;
            var overview = PowerOverview.From(country, world);

            _nationalValue.text = Format.Index(overview.NationalPower);
            _national.Set(Format.Index(overview.NationalPower), overview.NationalPower / 100f);
            _military.Set(Format.Index(overview.MilitaryReadiness), overview.MilitaryReadiness / 100f);
            _economic.Set(Format.Index(overview.EconomicPower), overview.EconomicPower / 100f);
            _technological.Set(Format.Index(overview.TechnologicalPower), overview.TechnologicalPower / 100f);
            _influence.Set(Format.Index(overview.GlobalInfluence), overview.GlobalInfluence / 100f);

            _economyRank.text = Loc.Get("power.rank_format", Rank(world, country, c => c.Gdp), world.Count);
            _militaryRank.text = Loc.Get("power.rank_format", Rank(world, country, c => c.MilitaryStrength), world.Count);
            _influenceRank.text = Loc.Get("power.rank_format", Rank(world, country, c => c.Influence), world.Count);
        }

        private static int Rank(IReadOnlyList<CountryState> world, CountryState country, System.Func<CountryState, double> metric)
        {
            var value = metric(country);
            var rank = 1;
            for (var i = 0; i < world.Count; i++)
            {
                if (world[i] != country && metric(world[i]) > value)
                {
                    rank++;
                }
            }

            return rank;
        }
    }
}
