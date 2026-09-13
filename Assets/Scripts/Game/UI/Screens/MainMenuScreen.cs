using Nation.Game.Bootstrap;
using Nation.Game.UI.Components;
using Nation.Game.UI.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Nation.Game.UI.Screens
{
    /// <summary>Cinematic entry screen. PLAY opens country selection; other sections are announced as upcoming.</summary>
    public sealed class MainMenuScreen : UIScreen
    {
        public MainMenuScreen(GameContext context) : base(context, "main-menu")
        {
        }

        protected override void Build(VisualElement root)
        {
            root.AddToClassList("menu");
            root.Add(new GlobeBackdropElement());

            var column = new VisualElement { name = "column" };
            column.AddToClassList("column");
            column.AddToClassList("menu__column");
            root.Add(column);

            var hero = new VisualElement();
            hero.AddToClassList("menu__hero");
            hero.Add(Typography.Eyebrow(Loc.Get("menu.eyebrow")));
            hero.Add(Typography.Display(Loc.Get("menu.title_line1"), "menu__title"));
            hero.Add(Typography.Title(Loc.Get("menu.title_line2"), "menu__title-accent"));
            var rule = new VisualElement();
            rule.AddToClassList("rule");
            hero.Add(rule);
            hero.Add(Typography.Caption(Loc.Get("menu.tagline"), "menu__tagline"));
            column.Add(hero);

            var menu = new VisualElement();
            menu.AddToClassList("menu__actions");
            menu.Add(Buttons.Primary(Loc.Get("menu.play"), OnPlay));
            menu.Add(Buttons.Secondary(Loc.Get("menu.multiplayer"), () => Announce("menu.multiplayer")));
            menu.Add(Buttons.Secondary(Loc.Get("menu.scenarios"), () => Announce("menu.scenarios")));
            menu.Add(Buttons.Secondary(Loc.Get("menu.profile"), () => Announce("menu.profile")));
            menu.Add(Buttons.Secondary(Loc.Get("menu.settings"), () => Announce("menu.settings")));
            column.Add(menu);

            column.Add(Typography.Caption(Loc.Get("menu.version", Application.version), "menu__version"));
        }

        private void OnPlay()
        {
            UI.Screens.Push(new CountrySelectScreen(Context));
        }

        private void Announce(string sectionKey)
        {
            var dialog = new ModalDialog()
                .WithTitle(Loc.Get(sectionKey))
                .WithBody(Loc.Get("common.coming_soon_body"))
                .AddAction(Loc.Get("common.close"), ModalDialog.ActionStyle.Secondary, null);
            UI.Modals.Show(dialog);
        }
    }
}
