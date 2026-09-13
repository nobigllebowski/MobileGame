using System;
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

    /// <summary>
    /// Single-scene navigation. Scenes hold 3D content (the future map); screens live in the persistent UI
    /// document, so a scene change does not tear the UI down.
    /// </summary>
    public sealed class SceneNavigator
    {
        private AsyncOperation _pending;

        public bool IsLoading => _pending != null && !_pending.isDone;

        public string ActiveSceneName => SceneManager.GetActiveScene().name;

        public event Action<string> LoadStarted;

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
                return;
            }

            LoadStarted?.Invoke(sceneName);
        }
    }
}
