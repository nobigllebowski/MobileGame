using System;
using System.Collections.Generic;
using Nation.Game.UI.Core;
using UnityEngine.UIElements;

namespace Nation.Game.UI.Components
{
    public enum GameTabId
    {
        Nation,
        Economy,
        Build,
        World,
        Power
    }

    /// <summary>Five-tab bottom navigation with icon, label and an animated active indicator.</summary>
    public sealed class BottomNavBar : VisualElement
    {
        private sealed class TabItem
        {
            public GameTabId Id;
            public VisualElement Root;
            public IconElement Icon;
            public Label Label;
            public VisualElement Indicator;
        }

        private readonly List<TabItem> _items = new List<TabItem>();

        public GameTabId Active { get; private set; }

        public event Action<GameTabId> TabSelected;

        public BottomNavBar(Func<GameTabId, string> labelFor)
        {
            name = "bottom-nav";
            AddToClassList("nav");

            AddTab(GameTabId.Nation, IconKind.Nation, labelFor(GameTabId.Nation));
            AddTab(GameTabId.Economy, IconKind.Economy, labelFor(GameTabId.Economy));
            AddTab(GameTabId.World, IconKind.World, labelFor(GameTabId.World));
            AddTab(GameTabId.Build, IconKind.Build, labelFor(GameTabId.Build));
            AddTab(GameTabId.Power, IconKind.Power, labelFor(GameTabId.Power));
        }

        public void SetActive(GameTabId id, bool notify = true)
        {
            Active = id;
            foreach (var item in _items)
            {
                var active = item.Id == id;
                item.Root.EnableInClassList("nav__tab--active", active);
                item.Icon.Color = active ? Palette.Accent : Palette.TextMuted;
            }

            if (notify)
            {
                TabSelected?.Invoke(id);
            }
        }

        private void AddTab(GameTabId id, IconKind icon, string label)
        {
            var root = new VisualElement { name = "nav-" + id.ToString().ToLowerInvariant() };
            root.AddToClassList("nav__tab");

            var indicator = new VisualElement();
            indicator.AddToClassList("nav__indicator");
            root.Add(indicator);

            var glyph = new IconElement(icon) { Color = Palette.TextMuted };
            glyph.AddToClassList("nav__icon");
            root.Add(glyph);

            var text = new Label(label);
            text.AddToClassList("nav__label");
            root.Add(text);

            root.RegisterCallback<ClickEvent>(_ =>
            {
                if (Active != id)
                {
                    SetActive(id);
                }
            });

            Add(root);
            _items.Add(new TabItem { Id = id, Root = root, Icon = glyph, Label = text, Indicator = indicator });
        }
    }
}
