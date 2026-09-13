using Nation.Game.Globe;
using Nation.Game.UI.Screens;
using UnityEngine;

namespace Nation.Game.Bootstrap
{
    /// <summary>Lives in the MainMenu scene. Shows the menu screen in the persistent UI.</summary>
    public sealed class MainMenuSceneController : MonoBehaviour
    {
        private void Start()
        {
            var context = GameContext.Current;
            if (context == null || context.UI == null)
            {
                Debug.LogError("[Menu] No game context. See earlier errors from GameBootstrap.");
                return;
            }

            var hasGlobe = false;
            if (context.Map.IsLoaded)
            {
                var tier = GlobeQuality.Resolve();
                EarthGlobe.Build(context.Map.Catalog, tier, Camera.main);
                hasGlobe = true;
                Debug.Log("[Menu] Earth globe built at quality tier " + tier + ".");
            }

            context.UI.Screens.ReplaceAll(new MainMenuScreen(context, hasGlobe));
        }
    }
}
