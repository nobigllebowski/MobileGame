using System;
using Nation.Core.Localization;
using Nation.Core.Signals;
using Nation.Game.Config;
using Nation.Game.Localization;
using Nation.Game.Scenes;
using Nation.Game.Session;
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
        public SceneNavigator Scenes { get; }

        /// <summary>The running game, or null while in the menu with no game started.</summary>
        public GameSession Session { get; private set; }

        public event Action<GameSession> SessionChanged;

        private GameContext(GameDataCatalog catalog)
        {
            Catalog = catalog;
            Signals = new SignalBus();
            Localization = BuildLocalization(catalog);
            Scenes = new SceneNavigator();
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

        internal static void Clear()
        {
            Current = null;
        }

        public GameSession StartNewGame(string playerCountryId)
        {
            var seed = Environment.TickCount;
            Session = GameSession.StartNew(playerCountryId, seed, Signals);
            Debug.Log("[Session] New game started as " + playerCountryId + " with seed " + seed + ".");
            SessionChanged?.Invoke(Session);
            return Session;
        }

        public void EndGame()
        {
            Session = null;
            SessionChanged?.Invoke(null);
        }

        private static ILocalizationService BuildLocalization(GameDataCatalog catalog)
        {
            var service = new StringTableLocalizationService();
            service.MissingKey += key => Debug.LogWarning("[Localization] Missing key '" + key + "' in locale '" + service.CurrentLocale + "'.");

            var tables = catalog.LocalizationTables;
            if (tables == null || tables.Length == 0)
            {
                Debug.LogError("[Localization] The Game Data Catalog has no localization tables assigned.");
                return service;
            }

            string firstLocale = null;
            foreach (var table in tables)
            {
                var locale = LocalizationTableLoader.LoadInto(service, table);
                if (firstLocale == null && locale != null)
                {
                    firstLocale = locale;
                }
            }

            if (firstLocale != null)
            {
                service.SetLocale(firstLocale);
            }

            return service;
        }
    }
}
