using Nation.Core.Localization;
using Nation.Game.Config;
using Nation.Game.Scenes;
using Nation.Game.Session;
using UnityEngine;
using UnityEngine.UIElements;

namespace Nation.Game.UI
{
    /// <summary>Main menu: PLAY starts a new game; every other section shows a COMING SOON card.</summary>
    public sealed class MainMenuController : ScreenController
    {
        private const string CardVisibleClass = "modal--visible";

        private VisualElement _comingSoonCard;
        private Label _comingSoonTitle;

        protected override VisualTreeAsset SelectLayout(GameDataCatalog catalog) => catalog.MainMenuLayout;

        protected override void OnScreenReady(VisualElement root)
        {
            var loc = Context.Localization;

            var backdropHost = root.Q<VisualElement>("backdrop");
            (backdropHost ?? root).Insert(0, new GlobeBackdropElement());

            SetText(root, "eyebrow", loc.Get("menu.eyebrow"));
            SetText(root, "title-line-1", loc.Get("menu.title_line1"));
            SetText(root, "title-line-2", loc.Get("menu.title_line2"));
            SetText(root, "tagline", loc.Get("menu.tagline"));
            SetText(root, "version", loc.Get("menu.version", Application.version));

            BindButton(root, "play-button", loc.Get("menu.play"), OnPlay);
            BindComingSoon(root, "multiplayer-button", "menu.multiplayer");
            BindComingSoon(root, "scenarios-button", "menu.scenarios");
            BindComingSoon(root, "profile-button", "menu.profile");
            BindComingSoon(root, "settings-button", "menu.settings");

            _comingSoonCard = root.Q<VisualElement>("coming-soon-modal");
            _comingSoonTitle = root.Q<Label>("coming-soon-title");
            SetText(root, "coming-soon-badge", loc.Get("common.coming_soon"));
            SetText(root, "coming-soon-body", loc.Get("common.coming_soon_body"));
            BindButton(root, "coming-soon-close", loc.Get("common.close"), HideComingSoon);

            var scrim = root.Q<VisualElement>("coming-soon-scrim");
            scrim?.RegisterCallback<ClickEvent>(_ => HideComingSoon());
        }

        private void OnPlay()
        {
            if (Context.Scenes.IsLoading)
            {
                return;
            }

            Context.StartNewGame(PlaceholderCountries.DefaultPlayerCountryId);
            Context.Scenes.GoTo(SceneNames.World);
        }

        private void BindComingSoon(VisualElement root, string buttonName, string labelKey)
        {
            var loc = Context.Localization;
            var label = loc.Get(labelKey);
            BindButton(root, buttonName, label, () => ShowComingSoon(label));
        }

        private void ShowComingSoon(string sectionTitle)
        {
            if (_comingSoonCard == null)
            {
                return;
            }

            if (_comingSoonTitle != null)
            {
                _comingSoonTitle.text = sectionTitle;
            }

            _comingSoonCard.style.display = DisplayStyle.Flex;
            // Deferred by one frame so the opacity transition plays from the hidden state.
            _comingSoonCard.schedule.Execute(() => _comingSoonCard.AddToClassList(CardVisibleClass));
        }

        private void HideComingSoon()
        {
            if (_comingSoonCard == null)
            {
                return;
            }

            _comingSoonCard.RemoveFromClassList(CardVisibleClass);
            _comingSoonCard.schedule.Execute(() => _comingSoonCard.style.display = DisplayStyle.None).StartingIn(220);
        }

        private static void SetText(VisualElement root, string elementName, string text)
        {
            var label = root.Q<Label>(elementName);
            if (label != null)
            {
                label.text = text;
            }
        }

        private static void BindButton(VisualElement root, string elementName, string text, System.Action onClick)
        {
            var button = root.Q<Button>(elementName);
            if (button == null)
            {
                Debug.LogWarning("[UI] Button '" + elementName + "' not found in layout.");
                return;
            }

            button.text = text;
            button.clicked += onClick;
        }
    }
}
