using System;
using System.Collections.Generic;
using Nation.Core.Buildings;
using Nation.Core.Countries;
using Nation.Core.Localization;
using Nation.Core.Session;
using Nation.Core.Signals;
using Nation.Game.Config;
using Nation.Game.Data;
using Nation.Game.Map;
using Nation.Game.Scenes;
using Nation.Game.UI.Core;
using UnityEngine;

namespace Nation.Game.Bootstrap
{
    /// <summary>
    /// The composed set of game-wide services. Built once by GameBootstrap and reached through Current.
    /// This is the project's one deliberate static: it replaces a DI container for a graph this small.
    /// </summary>
    public sealed class GameContext
    {
        public static GameContext Current { get; private set; }

        public GameDataCatalog Catalog { get; }
        public SignalBus Signals { get; }
        public ILocalizationService Localization { get; }
        public StringTableLocalizationService LocalizationTables { get; }
        public SceneNavigator Scenes { get; }
        public ICountryDataProvider Countries { get; }
        public IReadOnlyList<BuildingDefinition> Buildings { get; }
        public MapService Map { get; }
        public UIService UI { get; private set; }

        /// <summary>The running game, or null while in the menu with no game started.</summary>
        public GameSession Session { get; private set; }

        /// <summary>Country chosen in the selection flow. Survives until a different one is chosen.</summary>
        public string SelectedCountryId { get; private set; }

        public event Action<GameSession> SessionChanged;

        private GameContext(GameDataCatalog catalog)
        {
            Catalog = catalog;
            Signals = new SignalBus();
            LocalizationTables = BuildLocalization(catalog);
            Localization = LocalizationTables;
            Scenes = new SceneNavigator();
            Countries = StaticDataLoader.LoadCountries(catalog.Countries);
            Buildings = StaticDataLoader.LoadBuildings(catalog.Buildings);
            Map = new MapService(catalog.MapCatalog, () => Session != null ? Session.World : null, Signals);
        }

        internal static GameContext Create(GameDataCatalog catalog)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            Current = new GameContext(catalog);
            return Current;
        }

        internal void AttachUI(UIService ui)
        {
            UI = ui;
        }

        internal static void Clear()
        {
            Current = null;
        }

        public void SelectCountry(string countryId)
        {
            SelectedCountryId = countryId;
        }

        public GameSession StartNewGame(string playerCountryId)
        {
            SelectedCountryId = playerCountryId;
            var seed = Environment.TickCount;
            Session = GameSession.StartNew(playerCountryId, seed, Countries, Signals);
            Debug.Log("[Session] New game started as " + playerCountryId + " with seed " + seed + ".");
            SessionChanged?.Invoke(Session);
            return Session;
        }

        public void EndGame()
        {
            Session = null;
            SessionChanged?.Invoke(null);
        }

        private static StringTableLocalizationService BuildLocalization(GameDataCatalog catalog)
        {
            var service = new StringTableLocalizationService();
            service.MissingKey += key => Debug.LogWarning("[Localization] Missing key '" + key + "' in locale '" + service.CurrentLocale + "'.");

            var first = StaticDataLoader.LoadLocalization(service, catalog.LocalizationTables);
            if (first == null)
            {
                Debug.LogError("[Localization] No localization table could be loaded from the Game Data Catalog.");
            }
            else
            {
                service.SetLocale(first);
            }

            return service;
        }
    }
}
