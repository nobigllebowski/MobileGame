using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Nation.Game.UI.Core
{
    /// <summary>
    /// Navigation stack of full-screen views with fade-and-slide transitions (260 ms).
    /// Covered screens stay built but hidden, so returning to them is instant and free of rebuilds.
    /// </summary>
    public sealed class ScreenStack
    {
        public const long TransitionMs = 260;

        private const string EnterRightClass = "screen--enter-right";
        private const string EnterLeftClass = "screen--enter-left";
        private const string ExitRightClass = "screen--exit-right";
        private const string ExitFadeClass = "screen--exit-fade";

        private readonly VisualElement _layer;
        private readonly List<UIScreen> _screens = new List<UIScreen>();

        public UIScreen Current => _screens.Count > 0 ? _screens[_screens.Count - 1] : null;
        public int Count => _screens.Count;
        public bool IsTransitioning { get; private set; }

        public ScreenStack(VisualElement layer)
        {
            _layer = layer;
        }

        public void Push(UIScreen screen)
        {
            if (IsTransitioning)
            {
                return;
            }

            var previous = Current;
            _screens.Add(screen);
            Show(screen, EnterRightClass);
            if (previous != null)
            {
                previous.OnHidden();
                HideAfterTransition(previous);
            }
        }

        public void Pop()
        {
            if (IsTransitioning || _screens.Count <= 1)
            {
                return;
            }

            var leaving = Current;
            _screens.RemoveAt(_screens.Count - 1);
            var revealed = Current;

            revealed.Root.style.display = DisplayStyle.Flex;
            revealed.Root.AddToClassList(EnterLeftClass);
            revealed.Root.schedule.Execute(() => revealed.Root.RemoveFromClassList(EnterLeftClass));
            revealed.OnShown();

            leaving.OnHidden();
            leaving.Root.AddToClassList(ExitRightClass);
            Remove(leaving);
        }

        public void Replace(UIScreen screen)
        {
            if (IsTransitioning)
            {
                return;
            }

            var previous = Current;
            if (previous != null)
            {
                _screens.RemoveAt(_screens.Count - 1);
                previous.OnHidden();
                previous.Root.AddToClassList(ExitFadeClass);
                Remove(previous);
            }

            _screens.Add(screen);
            Show(screen, EnterRightClass);
        }

        /// <summary>Clears every screen and shows the given one. Used on scene changes.</summary>
        public void ReplaceAll(UIScreen screen)
        {
            IsTransitioning = false;
            foreach (var old in _screens)
            {
                old.OnHidden();
                old.Root.AddToClassList(ExitFadeClass);
                Remove(old);
            }

            _screens.Clear();
            _screens.Add(screen);
            Show(screen, EnterRightClass);
        }

        private void Show(UIScreen screen, string enterClass)
        {
            screen.EnsureBuilt();
            screen.Root.style.display = DisplayStyle.Flex;
            screen.Root.AddToClassList(enterClass);
            if (screen.Root.parent != _layer)
            {
                _layer.Add(screen.Root);
            }

            screen.Root.BringToFront();
            IsTransitioning = true;
            screen.Root.schedule.Execute(() => screen.Root.RemoveFromClassList(enterClass));
            screen.Root.schedule.Execute(() => IsTransitioning = false).StartingIn(TransitionMs);
            screen.OnShown();
        }

        private void HideAfterTransition(UIScreen screen)
        {
            screen.Root.schedule.Execute(() =>
            {
                if (_screens.Contains(screen) && screen != Current)
                {
                    screen.Root.style.display = DisplayStyle.None;
                }
            }).StartingIn(TransitionMs + 30);
        }

        private void Remove(UIScreen screen)
        {
            var root = screen.Root;
            root.schedule.Execute(() =>
            {
                root.RemoveFromHierarchy();
                screen.OnDestroyed();
            }).StartingIn(TransitionMs + 30);
        }
    }
}
