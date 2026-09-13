using System;
using Nation.Core.Countries;
using Nation.Core.Localization;
using Nation.Core.Nation;
using Nation.Game.UI.Formatting;
using UnityEngine.UIElements;

namespace Nation.Game.UI.Components
{
    /// <summary>List item for the country selection screen.</summary>
    public sealed class CountryCard : VisualElement
    {
        public CountryDefinition Country { get; }

        public CountryCard(CountryDefinition country, ILocalizationService loc, UiFormat format, Action<CountryDefinition> onSelect)
        {
            Country = country;
            AddToClassList("card");
            AddToClassList("country-card");

            var top = new VisualElement();
            top.AddToClassList("country-card__top");

            top.Add(new FlagElement(country.Flag, "flag--list"));

            var identity = new VisualElement();
            identity.AddToClassList("country-card__identity");
            identity.Add(Typography.Heading(loc.Get(country.NameKey), "country-card__name"));
            identity.Add(Typography.Caption(loc.Get(country.CapitalKey) + "  ·  " + loc.Get(country.RegionKey), "country-card__capital"));
            top.Add(identity);

            var difficulty = CountryDifficultyCalculator.Calculate(country);
            top.Add(new Badge(loc.Get(CountryDifficultyCalculator.Key(difficulty)), LevelFor(difficulty)));
            Add(top);

            var stats = new VisualElement();
            stats.AddToClassList("country-card__stats");
            stats.Add(Mini(loc.Get("stat.gdp"), format.Money(country.Gdp)));
            stats.Add(Mini(loc.Get("stat.population"), format.Population(country.Population)));
            stats.Add(Mini(loc.Get("stat.influence"), loc.Get(CountryTiers.Key(CountryTiers.Influence(country.Influence)))));
            Add(stats);

            var chevron = new IconElement(IconKind.ChevronRight) { Color = Core.Palette.TextMuted };
            chevron.AddToClassList("country-card__chevron");
            Add(chevron);

            RegisterCallback<ClickEvent>(_ => onSelect?.Invoke(country));
        }

        public static StatusLevel LevelFor(CountryDifficulty difficulty)
        {
            switch (difficulty)
            {
                case CountryDifficulty.Easy: return StatusLevel.Positive;
                case CountryDifficulty.Normal: return StatusLevel.Neutral;
                case CountryDifficulty.Hard: return StatusLevel.Warning;
                default: return StatusLevel.Danger;
            }
        }

        private static VisualElement Mini(string label, string value)
        {
            var box = new VisualElement();
            box.AddToClassList("mini-stat");
            box.Add(Typography.Caption(label, "mini-stat__label"));
            box.Add(Typography.Body(value, "mini-stat__value"));
            return box;
        }
    }
}
