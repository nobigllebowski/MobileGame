using System;
using System.Collections.Generic;
using Nation.Core.Countries;
using Nation.Game.Bootstrap;
using Nation.Game.UI.Components;
using Nation.Game.UI.Core;
using UnityEngine.UIElements;

namespace Nation.Game.UI.Screens
{
    /// <summary>CHOOSE YOUR NATION: search, filters and the country list.</summary>
    public sealed class CountrySelectScreen : UIScreen
    {
        private enum FilterKind { Region, Population, Economy, Technology, Influence, Difficulty }

        private sealed class Option
        {
            public string Label;
            public bool Selected;
            public Action Apply;
        }

        private readonly Dictionary<FilterKind, FilterChip> _chips = new Dictionary<FilterKind, FilterChip>();
        private CountryFilter _filter = CountryFilter.None;
        private string _query = string.Empty;
        private SearchField _search;
        private FilterChip _resetChip;
        private Label _count;
        private ScrollView _list;
        private VisualElement _empty;

        public CountrySelectScreen(GameContext context) : base(context, "country-select")
        {
        }

        protected override void Build(VisualElement root)
        {
            root.AddToClassList("select");

            var column = new VisualElement();
            column.AddToClassList("column");
            column.AddToClassList("select__column");
            root.Add(column);

            var header = new VisualElement();
            header.AddToClassList("screen-header");
            header.Add(Buttons.Icon(IconKind.Back, () => UI.Screens.Pop(), "screen-header__back"));
            var titles = new VisualElement();
            titles.AddToClassList("screen-header__titles");
            titles.Add(Typography.Title(Loc.Get("select.title")));
            titles.Add(Typography.Caption(Loc.Get("select.subtitle"), "screen-header__subtitle"));
            header.Add(titles);
            column.Add(header);

            _search = new SearchField(Loc.Get("select.search_placeholder"));
            _search.Changed += value =>
            {
                _query = value ?? string.Empty;
                Refresh();
            };
            column.Add(_search);

            var chipScroll = new ScrollView(ScrollViewMode.Horizontal);
            chipScroll.AddToClassList("chip-row");
            chipScroll.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            chipScroll.verticalScrollerVisibility = ScrollerVisibility.Hidden;
            _resetChip = new FilterChip(Loc.Get("select.filter.reset"), false);
            _resetChip.AddToClassList("chip--reset");
            _resetChip.Clicked += ResetFilters;
            _resetChip.style.display = DisplayStyle.None;
            chipScroll.Add(_resetChip);
            AddChip(chipScroll, FilterKind.Region, "select.filter.region");
            AddChip(chipScroll, FilterKind.Population, "select.filter.population");
            AddChip(chipScroll, FilterKind.Economy, "select.filter.economy");
            AddChip(chipScroll, FilterKind.Technology, "select.filter.technology");
            AddChip(chipScroll, FilterKind.Influence, "select.filter.influence");
            AddChip(chipScroll, FilterKind.Difficulty, "select.filter.difficulty");
            column.Add(chipScroll);

            _count = Typography.Caption(string.Empty, "select__count");
            column.Add(_count);

            _list = new ScrollView(ScrollViewMode.Vertical);
            _list.AddToClassList("select__list");
            _list.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            _list.verticalScrollerVisibility = ScrollerVisibility.Hidden;
            column.Add(_list);

            _empty = new EmptyState(IconKind.Search, Loc.Get("select.empty.title"), Loc.Get("select.empty.body"), Loc.Get("select.filter.reset"), () =>
            {
                _search.ClearText();
                ResetFilters();
            });
            _empty.style.display = DisplayStyle.None;
            column.Add(_empty);

            Refresh();
        }

        private void AddChip(VisualElement parent, FilterKind kind, string labelKey)
        {
            var chip = new FilterChip(Loc.Get(labelKey));
            chip.Clicked += () => OpenPicker(kind, Loc.Get(labelKey));
            _chips[kind] = chip;
            parent.Add(chip);
        }

        private void Refresh()
        {
            var results = CountryQuery.Apply(Context.Countries.Playable, _filter, _query, Loc);
            _list.Clear();
            foreach (var country in results)
            {
                _list.Add(new CountryCard(country, Loc, Format, OnCountryChosen));
            }

            _count.text = Loc.Get("select.count", results.Count);
            var empty = results.Count == 0;
            _empty.style.display = empty ? DisplayStyle.Flex : DisplayStyle.None;
            _list.style.display = empty ? DisplayStyle.None : DisplayStyle.Flex;
            _resetChip.style.display = _filter.IsEmpty ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private void ResetFilters()
        {
            _filter = CountryFilter.None;
            foreach (var chip in _chips.Values)
            {
                chip.SetValue(null);
            }

            Refresh();
        }

        private void OnCountryChosen(CountryDefinition country)
        {
            Context.SelectCountry(country.Id);
            UI.Toast(Loc.Get("toast.country_selected", Loc.Get(country.NameKey)));
            UI.Screens.Push(new CountryPreviewScreen(Context, country));
        }

        private void OpenPicker(FilterKind kind, string title)
        {
            var options = BuildOptions(kind);
            var sheet = new BottomSheet { AllowExpanded = false };
            sheet.SetSnapPoints(0.34f, 0.62f, 0.9f);
            sheet.Header.Add(Typography.SectionTitle(title, "sheet__title"));

            foreach (var option in options)
            {
                var captured = option;
                sheet.Body.Add(new OptionRow(captured.Label, captured.Selected, () =>
                {
                    captured.Apply();
                    UI.Sheets.Dismiss();
                    Refresh();
                }));
            }

            UI.Sheets.Show(sheet, options.Count > 5 ? SheetState.Half : SheetState.Peek);
        }

        private List<Option> BuildOptions(FilterKind kind)
        {
            var options = new List<Option>();
            var chip = _chips[kind];
            options.Add(new Option
            {
                Label = Loc.Get("select.filter.any"),
                Selected = !HasValue(kind),
                Apply = () => { ClearValue(kind); chip.SetValue(null); }
            });

            switch (kind)
            {
                case FilterKind.Region:
                    foreach (CountryRegion value in Enum.GetValues(typeof(CountryRegion)))
                    {
                        var captured = value;
                        var label = Loc.Get(CountryEnums.Key(captured));
                        options.Add(new Option { Label = label, Selected = _filter.Region == captured, Apply = () => { _filter.Region = captured; chip.SetValue(label); } });
                    }
                    break;
                case FilterKind.Population:
                    foreach (PopulationTier value in Enum.GetValues(typeof(PopulationTier)))
                    {
                        var captured = value;
                        var label = Loc.Get(CountryTiers.Key(captured));
                        options.Add(new Option { Label = label, Selected = _filter.Population == captured, Apply = () => { _filter.Population = captured; chip.SetValue(label); } });
                    }
                    break;
                case FilterKind.Economy:
                    foreach (EconomyTier value in Enum.GetValues(typeof(EconomyTier)))
                    {
                        var captured = value;
                        var label = Loc.Get(CountryTiers.Key(captured));
                        options.Add(new Option { Label = label, Selected = _filter.Economy == captured, Apply = () => { _filter.Economy = captured; chip.SetValue(label); } });
                    }
                    break;
                case FilterKind.Technology:
                    foreach (TechnologyTier value in Enum.GetValues(typeof(TechnologyTier)))
                    {
                        var captured = value;
                        var label = Loc.Get(CountryTiers.Key(captured));
                        options.Add(new Option { Label = label, Selected = _filter.Technology == captured, Apply = () => { _filter.Technology = captured; chip.SetValue(label); } });
                    }
                    break;
                case FilterKind.Influence:
                    foreach (InfluenceTier value in Enum.GetValues(typeof(InfluenceTier)))
                    {
                        var captured = value;
                        var label = Loc.Get(CountryTiers.Key(captured));
                        options.Add(new Option { Label = label, Selected = _filter.Influence == captured, Apply = () => { _filter.Influence = captured; chip.SetValue(label); } });
                    }
                    break;
                case FilterKind.Difficulty:
                    foreach (CountryDifficulty value in Enum.GetValues(typeof(CountryDifficulty)))
                    {
                        var captured = value;
                        var label = Loc.Get(CountryDifficultyCalculator.Key(captured));
                        options.Add(new Option { Label = label, Selected = _filter.Difficulty == captured, Apply = () => { _filter.Difficulty = captured; chip.SetValue(label); } });
                    }
                    break;
            }

            return options;
        }

        private bool HasValue(FilterKind kind)
        {
            switch (kind)
            {
                case FilterKind.Region: return _filter.Region != null;
                case FilterKind.Population: return _filter.Population != null;
                case FilterKind.Economy: return _filter.Economy != null;
                case FilterKind.Technology: return _filter.Technology != null;
                case FilterKind.Influence: return _filter.Influence != null;
                default: return _filter.Difficulty != null;
            }
        }

        private void ClearValue(FilterKind kind)
        {
            switch (kind)
            {
                case FilterKind.Region: _filter.Region = null; break;
                case FilterKind.Population: _filter.Population = null; break;
                case FilterKind.Economy: _filter.Economy = null; break;
                case FilterKind.Technology: _filter.Technology = null; break;
                case FilterKind.Influence: _filter.Influence = null; break;
                default: _filter.Difficulty = null; break;
            }
        }
    }
}
