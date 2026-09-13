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

            context.UI.Screens.ReplaceAll(new MainMenuScreen(context));
        }
    }
}
