using Nation.Game.Config;
using UnityEngine;

namespace Nation.Game.Bootstrap
{
    /// <summary>
    /// Composition root. Runs before the first scene loads, whichever scene that is, so pressing Play in any
    /// scene produces a fully initialised game. Creates a persistent host object that owns the context.
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
            Debug.Log("[Bootstrap] Game context ready. Locale: " + Context.Localization.CurrentLocale + ".");
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
