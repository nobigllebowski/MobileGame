using Nation.Core.Countries;
using Nation.Core.Models;
using Nation.Core.Nation;
using Nation.Game.Bootstrap;
using Nation.Game.Scenes;
using Nation.Game.UI.Components;
using Nation.Game.UI.Core;
using UnityEngine.UIElements;

namespace Nation.Game.UI.Screens
{
    /// <summary>Detailed briefing on one country with the pinned START LEADERSHIP action.</summary>
    public sealed class CountryPreviewScreen : UIScreen
    {
        private readonly CountryDefinition _country;

        public CountryPreviewScreen(GameContext context, CountryDefinition country) : base(context, "country-preview")
        {
            _country = country;
        }

        protected override void Build(VisualElement root)
        {
            root.AddToClassList("preview");

            var column = new VisualElement();
            column.AddToClassList("column");
            column.AddToClassList("preview__column");
            root.Add(column);

            var header = new VisualElement();
            header.AddToClassList("screen-header");
            header.Add(Buttons.Icon(IconKind.Back, () => UI.Screens.Pop(), "screen-header__back"));
            var titles = new VisualElement();
            titles.AddToClassList("screen-header__titles");
            titles.Add(Typography.Caption(Loc.Get("preview.eyebrow"), "screen-header__subtitle"));
            header.Add(titles);
            column.Add(header);

            var scroll = new ScrollView(ScrollViewMode.Vertical);
            scroll.AddToClassList("preview__scroll");
            scroll.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            scroll.verticalScrollerVisibility = ScrollerVisibility.Hidden;
            column.Add(scroll);

            var hero = new VisualElement();
            hero.AddToClassList("preview__hero");
            hero.Add(new FlagElement(_country.Flag, "flag--hero"));
            hero.Add(Typography.Display(Loc.Get(_country.NameKey), "preview__name"));
            hero.Add(Typography.Caption(Loc.Get(_country.GovernmentKey) + "  ·  " + Loc.Get(_country.RegionKey), "preview__meta"));

            var difficulty = CountryDifficultyCalculator.Calculate(_country);
            var badgeRow = new VisualElement();
            badgeRow.AddToClassList("preview__badges");
            badgeRow.Add(new Badge(Loc.Get("preview.difficulty") + "  " + Loc.Get(CountryDifficultyCalculator.Key(difficulty)), CountryCard.LevelFor(difficulty)));
            hero.Add(badgeRow);
            scroll.Add(hero);

            var grid = new VisualElement();
            grid.AddToClassList("grid-2");
            grid.Add(new StatCard(Loc.Get("stat.capital"), Loc.Get(_country.CapitalKey)));
            grid.Add(new StatCard(Loc.Get("stat.population"), Format.Population(_country.Population)));
            grid.Add(new StatCard(Loc.Get("stat.gdp"), Format.Money(_country.Gdp)));
            grid.Add(new StatCard(Loc.Get("stat.treasury"), Format.Money(_country.Treasury, 0)).Variant("stat-card--gold"));
            grid.Add(new StatCard(Loc.Get("stat.technology"), Loc.Get(CountryTiers.Key(CountryTiers.Technology(_country.Technology)))));
            grid.Add(new StatCard(Loc.Get("stat.stability"), Format.Percent(_country.Stability)));
            grid.Add(new StatCard(Loc.Get("stat.influence"), Loc.Get(CountryTiers.Key(CountryTiers.Influence(_country.Influence)))));
            grid.Add(new StatCard(Loc.Get("stat.area"), Format.Area(_country.AreaKm2)));
            scroll.Add(grid);

            var strengths = new InfoCard(Loc.Get("preview.strengths"));
            foreach (var key in _country.StrengthKeys)
            {
                strengths.AddBullet(Loc.Get(key), IconKind.Check, Palette.Positive);
            }
            scroll.Add(strengths);

            var challenges = new InfoCard(Loc.Get("preview.challenges"));
            foreach (var key in _country.ChallengeKeys)
            {
                challenges.AddBullet(Loc.Get(key), IconKind.Alert, Palette.Warning);
            }
            scroll.Add(challenges);

            var resources = new InfoCard(Loc.Get("preview.resources"), Loc.Get("preview.resources_hint"));
            resources.Content.Add(Gauge("resource.energy", _country.EnergySelfSufficiency));
            resources.Content.Add(Gauge("resource.oil", _country.Oil.SelfSufficiency));
            resources.Content.Add(Gauge("resource.gas", _country.Gas.SelfSufficiency));
            resources.Content.Add(Gauge("resource.food", _country.Food.SelfSufficiency));
            resources.Content.Add(Gauge("resource.iron", _country.Iron.SelfSufficiency));
            scroll.Add(resources);

            var spacer = new VisualElement();
            spacer.AddToClassList("preview__spacer");
            scroll.Add(spacer);

            var footer = new VisualElement();
            footer.AddToClassList("pinned-footer");
            footer.Add(Buttons.Primary(Loc.Get("preview.start"), StartLeadership));
            column.Add(footer);
        }

        private GaugeBar Gauge(string key, double sufficiency)
        {
            var variant = sufficiency >= 1.0 ? "gauge--positive" : sufficiency >= 0.9 ? "gauge--warning" : "gauge--danger";
            return new GaugeBar(Loc.Get(key), Format.Percent(sufficiency * 100.0), (float)(sufficiency / 1.5), variant);
        }

        private void StartLeadership()
        {
            if (Context.Scenes.IsLoading)
            {
                return;
            }

            Context.StartNewGame(_country.Id);
            UI.SetLoading(true);
            Context.Scenes.GoTo(SceneNames.World);
        }
    }
}
