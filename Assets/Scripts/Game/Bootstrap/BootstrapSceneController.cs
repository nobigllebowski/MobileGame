using Nation.Game.Scenes;
using UnityEngine;

namespace Nation.Game.Bootstrap
{
    /// <summary>
    /// Lives in the Bootstrap scene (build index 0). Once the context exists it hands over to the main menu.
    /// A loading screen will slot in here later without touching the bootstrap itself.
    /// </summary>
    public sealed class BootstrapSceneController : MonoBehaviour
    {
        private void Start()
        {
            if (GameContext.Current == null)
            {
                Debug.LogError("[Bootstrap] No game context. See earlier errors from GameBootstrap.");
                return;
            }

            GameContext.Current.Scenes.GoTo(SceneNames.MainMenu);
        }
    }
}
