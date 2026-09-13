using Nation.Core.Localization;
using Nation.Core.Models;
using Nation.Core.Session;
using Nation.Game.Bootstrap;
using Nation.Game.UI.Core;
using Nation.Game.UI.Formatting;
using Nation.Game.UI.Screens;
using UnityEngine.UIElements;

namespace Nation.Game.UI.Tabs
{
    /// <summary>
    /// One tab of the game shell. Built lazily on first show; refreshed only when the simulation signals.
    /// Tabs read the session and never mutate world state.
    /// </summary>
    public abstract class GameTab : VisualElement
    {
        protected GameContext Context { get; }
        protected GameShellScreen Shell { get; }
        protected ILocalizationService Loc => Context.Localization;
        protected UiFormat Format => Context.UI.Format;
        protected UIService UI => Context.UI;
        protected GameSession Session => Context.Session;

        public bool IsBuilt { get; private set; }

        protected GameTab(GameContext context, GameShellScreen shell, string tabName)
        {
            Context = context;
            Shell = shell;
            name = tabName;
            AddToClassList("tab");
        }

        public void EnsureBuilt()
        {
            if (IsBuilt)
            {
                return;
            }

            IsBuilt = true;
            Build();
        }

        protected abstract void Build();

        public virtual void OnShow()
        {
        }

        public virtual void OnHide()
        {
        }

        public virtual void OnTick()
        {
        }

        public virtual void OnSpeedChanged(GameSpeed speed)
        {
        }

        /// <summary>The shell is being torn down (scene change). Release scene references here.</summary>
        public virtual void OnDestroyed()
        {
        }

        protected ScrollView Scroll(string extraClass)
        {
            var scroll = new ScrollView(ScrollViewMode.Vertical);
            scroll.AddToClassList("tab__scroll");
            if (!string.IsNullOrEmpty(extraClass))
            {
                scroll.AddToClassList(extraClass);
            }

            scroll.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            scroll.verticalScrollerVisibility = ScrollerVisibility.Hidden;
            return scroll;
        }

        protected VisualElement TabHeader(string title, string subtitle)
        {
            var header = new VisualElement();
            header.AddToClassList("tab-header");
            header.Add(Components.Typography.Eyebrow(title, "tab-header__title"));
            if (!string.IsNullOrEmpty(subtitle))
            {
                header.Add(Components.Typography.Caption(subtitle, "tab-header__subtitle"));
            }

            return header;
        }
    }
}
