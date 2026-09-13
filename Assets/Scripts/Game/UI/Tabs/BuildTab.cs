using System.Collections.Generic;
using Nation.Core.Buildings;
using Nation.Game.Bootstrap;
using Nation.Game.UI.Components;
using Nation.Game.UI.Core;
using Nation.Game.UI.Screens;
using UnityEngine.UIElements;

namespace Nation.Game.UI.Tabs
{
    /// <summary>Project catalogue by category. CONSTRUCT is wired but the construction system arrives later.</summary>
    public sealed class BuildTab : GameTab
    {
        private readonly Dictionary<BuildingCategory?, FilterChip> _chips = new Dictionary<BuildingCategory?, FilterChip>();
        private VisualElement _list;

        public BuildTab(GameContext context, GameShellScreen shell) : base(context, shell, "tab-build")
        {
        }

        protected override void Build()
        {
            var scroll = Scroll("build");
            Add(scroll);

            scroll.Add(TabHeader(Loc.Get("build.title"), Loc.Get("build.subtitle")));

            var chipScroll = new ScrollView(ScrollViewMode.Horizontal);
            chipScroll.AddToClassList("chip-row");
            chipScroll.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            chipScroll.verticalScrollerVisibility = ScrollerVisibility.Hidden;
            AddChip(chipScroll, null, Loc.Get("build.category.all"));
            AddChip(chipScroll, BuildingCategory.Energy, Loc.Get(BuildingDefinition.CategoryKey(BuildingCategory.Energy)));
            AddChip(chipScroll, BuildingCategory.Industry, Loc.Get(BuildingDefinition.CategoryKey(BuildingCategory.Industry)));
            AddChip(chipScroll, BuildingCategory.Infrastructure, Loc.Get(BuildingDefinition.CategoryKey(BuildingCategory.Infrastructure)));
            AddChip(chipScroll, BuildingCategory.Housing, Loc.Get(BuildingDefinition.CategoryKey(BuildingCategory.Housing)));
            AddChip(chipScroll, BuildingCategory.Agriculture, Loc.Get(BuildingDefinition.CategoryKey(BuildingCategory.Agriculture)));
            scroll.Add(chipScroll);

            _list = new VisualElement();
            _list.AddToClassList("build__list");
            scroll.Add(_list);

            var spacer = new VisualElement();
            spacer.AddToClassList("tab__spacer");
            scroll.Add(spacer);

            SelectCategory(null);
        }

        private void AddChip(VisualElement parent, BuildingCategory? category, string label)
        {
            var chip = new FilterChip(label, false);
            chip.Clicked += () => SelectCategory(category);
            _chips[category] = chip;
            parent.Add(chip);
        }

        private void SelectCategory(BuildingCategory? category)
        {
            foreach (var pair in _chips)
            {
                pair.Value.SetSelected(pair.Key == category);
            }

            _list.Clear();
            var any = false;
            foreach (var building in Context.Buildings)
            {
                if (category != null && building.Category != category.Value)
                {
                    continue;
                }

                any = true;
                _list.Add(ProjectCard(building));
            }

            if (!any)
            {
                _list.Add(new EmptyState(IconKind.Build, Loc.Get("build.empty.title"), Loc.Get("build.empty.body")));
            }
        }

        private VisualElement ProjectCard(BuildingDefinition building)
        {
            var card = new VisualElement();
            card.AddToClassList("card");
            card.AddToClassList("project-card");

            var head = new VisualElement();
            head.AddToClassList("project-card__head");
            var titles = new VisualElement();
            titles.AddToClassList("project-card__titles");
            titles.Add(Typography.Heading(Loc.Get(building.NameKey), "project-card__name"));
            titles.Add(Typography.Caption(Loc.Get(BuildingDefinition.CategoryKey(building.Category)), "project-card__category"));
            head.Add(titles);
            card.Add(head);

            card.Add(Typography.Caption(Loc.Get(building.DescriptionKey), "project-card__description"));

            var stats = new VisualElement();
            stats.AddToClassList("project-card__stats");
            stats.Add(Mini(Loc.Get("build.cost"), Format.Money(building.Cost)));
            stats.Add(Mini(Loc.Get("build.duration"), Format.Days(building.DurationDays)));
            stats.Add(Mini(Loc.Get(BuildingDefinition.EffectKey(building.EffectKind)), "+" + Format.Compact(building.EffectValue, 0) + " " + Loc.Get(building.EffectUnitKey)));
            card.Add(stats);

            card.Add(Buttons.Secondary(Loc.Get("build.construct"), () => UI.ToastKey("toast.feature_unavailable", ToastKind.Warning)));
            return card;
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
