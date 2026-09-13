using System;
using System.Collections.Generic;
using Nation.Core.Models;
using Nation.Game.UI.Core;
using UnityEngine.UIElements;

namespace Nation.Game.UI.Components
{
    /// <summary>Pill with the five speed buttons. Raises SpeedRequested; the session decides.</summary>
    public sealed class TimeControls : VisualElement
    {
        private readonly Dictionary<GameSpeed, Button> _buttons = new Dictionary<GameSpeed, Button>();

        public event Action<GameSpeed> SpeedRequested;

        public TimeControls()
        {
            name = "time-controls";
            AddToClassList("time");
            AddSpeed(GameSpeed.Paused, IconKind.Pause);
            AddSpeed(GameSpeed.Slow, IconKind.Slow);
            AddSpeed(GameSpeed.Normal, IconKind.Play);
            AddSpeed(GameSpeed.Fast, IconKind.Fast);
            AddSpeed(GameSpeed.VeryFast, IconKind.VeryFast);
        }

        public void SetSpeed(GameSpeed speed)
        {
            foreach (var pair in _buttons)
            {
                var active = pair.Key == speed;
                pair.Value.EnableInClassList("time__button--active", active);
                Buttons.GlyphOf(pair.Value).Color = active ? Palette.Text : Palette.TextMuted;
            }
        }

        private void AddSpeed(GameSpeed speed, IconKind icon)
        {
            var captured = speed;
            var button = Buttons.Icon(icon, () => SpeedRequested?.Invoke(captured), "time__button");
            button.name = "speed-" + speed.ToString().ToLowerInvariant();
            _buttons[speed] = button;
            Add(button);
        }
    }
}
