using Nation.Game.Map;
using Nation.Game.UI.Screens;
using UnityEngine;

namespace Nation.Game.Bootstrap
{
    /// <summary>
    /// Lives in the World scene. Shows the game shell. Pressing Play here directly starts a game with the
    /// first playable country so the scene stays testable on its own.
    /// </summary>
    public sealed class WorldSceneController : MonoBehaviour
    {
        private void Start()
        {
            var context = GameContext.Current;
            if (context == null || context.UI == null)
            {
                Debug.LogError("[World] No game context. See earlier errors from GameBootstrap.");
                return;
            }

            if (context.Session == null)
            {
                var playable = context.Countries.Playable;
                if (playable.Count == 0)
                {
                    Debug.LogError("[World] No playable countries loaded; cannot start a session.");
                    return;
                }

                context.StartNewGame(playable[0].Id);
            }

            MapWorldBuilder.Result map = null;
            if (context.Map.IsLoaded)
            {
                map = MapWorldBuilder.Build(context);
            }
            else
            {
                Debug.LogError("[World] Map catalog is empty; the world view will show no geography. Run Nation > Map > Import Natural Earth.");
            }

            context.UI.Screens.ReplaceAll(new GameShellScreen(context, map));
        }
    }
}
