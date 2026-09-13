using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nation.Game.Scenes
{
    /// <summary>Scene names as they appear in Build Settings.</summary>
    public static class SceneNames
    {
        public const string Bootstrap = "Bootstrap";
        public const string MainMenu = "MainMenu";
        public const string World = "World";
    }

    /// <summary>Single-scene navigation. Transitions and loading screens arrive with the UI foundation phase.</summary>
    public sealed class SceneNavigator
    {
        private AsyncOperation _pending;

        public bool IsLoading => _pending != null && !_pending.isDone;

        public string ActiveSceneName => SceneManager.GetActiveScene().name;

        public void GoTo(string sceneName)
        {
            if (IsLoading)
            {
                Debug.LogWarning("[Scenes] Ignoring request to load '" + sceneName + "' while another load is in progress.");
                return;
            }

            _pending = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            if (_pending == null)
            {
                Debug.LogError("[Scenes] Scene '" + sceneName + "' could not be loaded. Is it added to File > Build Profiles / Build Settings?");
            }
        }
    }
}
