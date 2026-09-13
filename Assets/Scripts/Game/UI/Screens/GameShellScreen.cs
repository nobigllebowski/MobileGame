using System;
using System.Collections.Generic;
using Nation.Core.Signals;
using Nation.Game.Bootstrap;
using Nation.Game.Map;
using Nation.Game.Scenes;
using Nation.Game.UI.Components;
using Nation.Game.UI.Core;
using Nation.Game.UI.Tabs;
using UnityEngine.UIElements;

namespace Nation.Game.UI.Screens
{
    /// <summary>
    /// The in-game frame: a content area holding the five tabs and the bottom navigation.
    /// Tabs are built once and toggled; the visible tab refreshes on simulation signals only.
    /// </summary>
    public sealed class GameShellScreen : UIScreen
    {
        private const string TabEnterClass = "tab--enter";

        private readonly Dictionary<GameTabId, GameTab> _tabs = new Dictionary<GameTabId, GameTab>();
        private VisualElement _content;
        private BottomNavBar _nav;
        private GameTab _active;
        private IDisposable _tickSubscription;
        private IDisposable _speedSubscription;
        private bool _welcomed;
        private readonly MapWorldBuilder.Result _map;

        public GameShellScreen(GameContext context, MapWorldBuilder.Result map = null) : base(context, "game-shell")
        {
            _map = map;
        }

        protected override void Build(VisualElement root)
        {
            root.AddToClassList("shell");

            _content = new VisualElement { name = "tab-content" };
            _content.AddToClassList("shell__content");
            root.Add(_content);

            _nav = new BottomNavBar(id => Loc.Get("nav." + id.ToString().ToLowerInvariant()));
            _nav.TabSelected += ShowTab;
            root.Add(_nav);

            _tabs[GameTabId.Nation] = new NationTab(Context, this);
            _tabs[GameTabId.Economy] = new EconomyTab(Context, this);
            _tabs[GameTabId.Build] = new BuildTab(Context, this);
            _tabs[GameTabId.World] = new WorldTab(Context, this, _map);
            _tabs[GameTabId.Power] = new PowerTab(Context, this);

            foreach (var tab in _tabs.Values)
            {
                tab.style.display = DisplayStyle.None;
                _content.Add(tab);
            }

            _nav.SetActive(GameTabId.World, false);
            ShowTab(GameTabId.World);
        }

        public override void OnShown()
        {
            UI.SetLoading(false);
            _tickSubscription = Context.Signals.Subscribe<TickCompletedSignal>(OnTick);
            _speedSubscription = Context.Signals.Subscribe<GameSpeedChangedSignal>(OnSpeedChanged);
            _active?.OnShow();

            if (!_welcomed && Context.Session != null)
            {
                _welcomed = true;
                UI.Toast(Loc.Get("toast.now_leading", Loc.Get(Context.Session.PlayerDefinition.NameKey)), ToastKind.Success);
            }
        }

        public override void OnHidden()
        {
            _tickSubscription?.Dispose();
            _speedSubscription?.Dispose();
            _tickSubscription = null;
            _speedSubscription = null;
            _active?.OnHide();
        }

        public void ShowTab(GameTabId id)
        {
            if (!_tabs.TryGetValue(id, out var next) || next == _active)
            {
                return;
            }

            if (_active != null)
            {
                _active.OnHide();
                _active.style.display = DisplayStyle.None;
            }

            _active = next;
            next.EnsureBuilt();
            next.style.display = DisplayStyle.Flex;
            next.AddToClassList(TabEnterClass);
            next.schedule.Execute(() => next.RemoveFromClassList(TabEnterClass));
            next.OnShow();
        }

        public void RequestMainMenu()
        {
            var dialog = new ModalDialog()
                .WithTitle(Loc.Get("dialog.leave.title"))
                .WithBody(Loc.Get("dialog.leave.body"))
                .AddAction(Loc.Get("common.cancel"), ModalDialog.ActionStyle.Secondary, null)
                .AddAction(Loc.Get("dialog.leave.confirm"), ModalDialog.ActionStyle.Danger, LeaveToMenu);
            UI.Modals.Show(dialog);
        }

        private void LeaveToMenu()
        {
            if (Context.Scenes.IsLoading)
            {
                return;
            }

            Context.EndGame();
            UI.SetLoading(true);
            Context.Scenes.GoTo(SceneNames.MainMenu);
        }

        public override void OnDestroyed()
        {
            foreach (var tab in _tabs.Values)
            {
                tab.OnDestroyed();
            }
        }

        private void OnTick(TickCompletedSignal signal)
        {
            _active?.OnTick();
        }

        private void OnSpeedChanged(GameSpeedChangedSignal signal)
        {
            _active?.OnSpeedChanged(signal.Speed);
        }
    }
}
