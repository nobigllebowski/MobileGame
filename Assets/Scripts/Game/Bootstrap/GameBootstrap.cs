using Nation.Game.Config;
using Nation.Game.Performance;
using Nation.Game.Time;
using Nation.Game.UI.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Nation.Game.Bootstrap
{
    /// <summary>
    /// Composition root. Runs before the first scene loads, whichever scene that is, so pressing Play in any
    /// scene produces a fully initialised game. Creates one persistent host object that owns the context,
    /// the simulation runner and the UI document that every screen renders into.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GameBootstrap : MonoBehaviour
    {
        private const int TargetFrameRate = 60;

        public GameContext Context { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            // Keeps the project correct when "Enter Play Mode Options" disables domain reload.
            GameContext.Clear();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeBeforeFirstScene()
        {
            if (GameContext.Current != null)
            {
                return;
            }

            var catalog = Resources.Load<GameDataCatalog>(GameDataCatalog.ResourcePath);
            if (catalog == null)
            {
                Debug.LogError("[Bootstrap] GameDataCatalog not found at Resources/" + GameDataCatalog.ResourcePath + ". The game cannot start.");
                return;
            }

            var host = new GameObject("[Nation] Game");
            DontDestroyOnLoad(host);
            host.AddComponent<GameBootstrap>().Build(catalog);
        }

        private void Build(GameDataCatalog catalog)
        {
            Application.targetFrameRate = TargetFrameRate;
            Context = GameContext.Create(catalog);

            var document = gameObject.AddComponent<UIDocument>();
            document.panelSettings = catalog.PanelSettings != null ? catalog.PanelSettings : UIService.CreateFallbackPanelSettings();
            var ui = new UIService(document, catalog, Context.Localization);
            Context.AttachUI(ui);

            gameObject.AddComponent<GameRunner>();
            gameObject.AddComponent<PerformanceMonitor>();

            Debug.Log("[Bootstrap] Game context ready. Locale: " + Context.Localization.CurrentLocale + ", countries: " + Context.Countries.All.Count + ".");
        }

        private void OnDestroy()
        {
            if (Context != null && ReferenceEquals(GameContext.Current, Context))
            {
                GameContext.Clear();
            }
        }
    }
}
