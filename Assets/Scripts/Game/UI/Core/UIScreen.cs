using Nation.Core.Localization;
using Nation.Game.Bootstrap;
using Nation.Game.UI.Formatting;
using UnityEngine.UIElements;

namespace Nation.Game.UI.Core
{
    /// <summary>
    /// One full-screen view managed by the ScreenStack. Screens build their tree in code from reusable
    /// components and read game state; they never mutate it directly.
    /// </summary>
    public abstract class UIScreen
    {
        public const string RootClass = "screen";

        protected GameContext Context { get; }
        protected ILocalizationService Loc => Context.Localization;
        protected UiFormat Format => Context.UI.Format;
        protected UIService UI => Context.UI;

        public VisualElement Root { get; }

        public bool IsBuilt { get; private set; }

        protected UIScreen(GameContext context, string name)
        {
            Context = context;
            Root = new VisualElement { name = name };
            Root.AddToClassList(RootClass);
        }

        internal void EnsureBuilt()
        {
            if (IsBuilt)
            {
                return;
            }

            IsBuilt = true;
            Build(Root);
        }

        /// <summary>Creates the screen's element tree. Called once, right before the first show.</summary>
        protected abstract void Build(VisualElement root);

        /// <summary>The screen became the visible top of the stack (first show or return from a popped screen).</summary>
        public virtual void OnShown()
        {
        }

        /// <summary>Another screen covered this one or it was removed. Release subscriptions here.</summary>
        public virtual void OnHidden()
        {
        }

        /// <summary>The screen was removed from the stack for good.</summary>
        public virtual void OnDestroyed()
        {
        }
    }
}
