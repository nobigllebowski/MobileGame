using Nation.Core.Models;
using Nation.Game.Bootstrap;
using UnityEngine;

namespace Nation.Game.Time
{
    /// <summary>
    /// The only per-frame hook into the simulation. Feeds elapsed real time to the session, which converts it
    /// into whole days according to the chosen speed. Pauses the game when the app goes to the background.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GameRunner : MonoBehaviour
    {
        private void Update()
        {
            var session = GameContext.Current != null ? GameContext.Current.Session : null;
            if (session == null || session.Speed == GameSpeed.Paused)
            {
                return;
            }

            session.AdvanceRealTime(UnityEngine.Time.unscaledDeltaTime);
        }

        private void OnApplicationPause(bool paused)
        {
            if (!paused)
            {
                return;
            }

            var session = GameContext.Current != null ? GameContext.Current.Session : null;
            if (session != null)
            {
                session.SetSpeed(GameSpeed.Paused);
            }
        }
    }
}
