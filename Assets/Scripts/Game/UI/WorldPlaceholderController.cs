using System;
using Nation.Core.Localization;
using Nation.Core.Models;
using Nation.Core.Signals;
using Nation.Core.Utilities;
using Nation.Game.Config;
using Nation.Game.Scenes;
using Nation.Game.Session;
using UnityEngine;
using UnityEngine.UIElements;

namespace Nation.Game.UI
{
    /// <summary>
    /// PHASE 1 PLACEHOLDER for the world screen: shows the player's country, the date and the primary metrics,
    /// and lets the player advance the simulation one day at a time. The real HUD and map replace it in Phase 5.
    /// </summary>
    public sealed class WorldPlaceholderController : ScreenController
    {
        private Label _dateLabel;
        private Label _treasuryValue;
        private Label _gdpValue;
        private Label _populationValue;
        private Label _tickLabel;
        private IDisposable _tickSubscription;
        private CompactSuffixes _suffixes;
        private string _currencySymbol;

        protected override VisualTreeAsset SelectLayout(GameDataCatalog catalog) => catalog.WorldPlaceholderLayout;

        protected override void OnScreenReady(VisualElement root)
        {
            var loc = Context.Localization;

            if (Context.Session == null)
            {
                // Pressing Play directly in the World scene is a valid developer workflow.
                Context.StartNewGame(PlaceholderCountries.DefaultPlayerCountryId);
            }

            _suffixes = new CompactSuffixes(
                loc.Get("number.suffix.thousand"),
                loc.Get("number.suffix.million"),
                loc.Get("number.suffix.billion"),
                loc.Get("number.suffix.trillion"));
            _currencySymbol = loc.Get("currency.symbol");

            var country = Context.Session.PlayerCountry;
            SetText(root, "country-name", loc.Get("country." + country.Id));
            SetText(root, "treasury-label", loc.Get("hud.treasury"));
            SetText(root, "gdp-label", loc.Get("hud.gdp"));
            SetText(root, "population-label", loc.Get("hud.population"));

            _dateLabel = root.Q<Label>("date");
            _treasuryValue = root.Q<Label>("treasury-value");
            _gdpValue = root.Q<Label>("gdp-value");
            _populationValue = root.Q<Label>("population-value");
            _tickLabel = root.Q<Label>("tick-info");

            var advance = root.Q<Button>("advance-day-button");
            if (advance != null)
            {
                advance.text = loc.Get("debug.advance_day");
                advance.clicked += OnAdvanceDay;
            }

            var menu = root.Q<Button>("menu-button");
            if (menu != null)
            {
                menu.text = loc.Get("common.menu");
                menu.clicked += OnReturnToMenu;
            }

            _tickSubscription = Context.Signals.Subscribe<TickCompletedSignal>(OnTickCompleted);
            Refresh();
        }

        protected override void OnScreenClosing()
        {
            _tickSubscription?.Dispose();
            _tickSubscription = null;
        }

        private void OnAdvanceDay()
        {
            Context.Session.TickOnce();
        }

        private void OnReturnToMenu()
        {
            if (Context.Scenes.IsLoading)
            {
                return;
            }

            Context.EndGame();
            Context.Scenes.GoTo(SceneNames.MainMenu);
        }

        private void OnTickCompleted(TickCompletedSignal signal)
        {
            Refresh();
        }

        private void Refresh()
        {
            var loc = Context.Localization;
            var world = Context.Session.World;
            var country = Context.Session.PlayerCountry;

            if (_dateLabel != null)
            {
                _dateLabel.text = FormatDate(loc, world.CurrentDate);
            }

            if (_treasuryValue != null)
            {
                _treasuryValue.text = _currencySymbol + NumberFormatting.FormatCompact(country.Treasury, _suffixes);
            }

            if (_gdpValue != null)
            {
                _gdpValue.text = _currencySymbol + NumberFormatting.FormatCompact(country.Gdp, _suffixes);
            }

            if (_populationValue != null)
            {
                _populationValue.text = NumberFormatting.FormatCompact(country.Population, _suffixes);
            }

            if (_tickLabel != null)
            {
                _tickLabel.text = loc.Get("debug.tick_info", world.TickCount, Context.Session.Engine.LastTickMilliseconds.ToString("F2"));
            }
        }

        private static string FormatDate(ILocalizationService loc, GameDate date)
        {
            var monthName = loc.Get("month." + date.Month);
            return loc.Get("date.long", monthName, date.Day, date.Year);
        }

        private static void SetText(VisualElement root, string elementName, string text)
        {
            var label = root.Q<Label>(elementName);
            if (label != null)
            {
                label.text = text;
            }
        }
    }
}
