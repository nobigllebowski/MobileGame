using Nation.Game.Bootstrap;
using Nation.Game.Config;
using UnityEngine;
using UnityEngine.UIElements;

namespace Nation.Game.UI
{
    /// <summary>
    /// Base for one full-screen UI Toolkit view. Ensures a UIDocument exists on the same object, assigns the
    /// shared panel settings and the layout from the catalog, then hands subclasses the root element.
    /// Screens that live on their own scene need no manual Inspector wiring at all.
    /// </summary>
    [DisallowMultipleComponent]
    public abstract class ScreenController : MonoBehaviour
    {
        private UIDocument _document;

        protected GameContext Context => GameContext.Current;
        protected VisualElement Root => _document != null ? _document.rootVisualElement : null;

        protected abstract VisualTreeAsset SelectLayout(GameDataCatalog catalog);

        /// <summary>Called once the layout is instantiated. Query elements and bind here.</summary>
        protected abstract void OnScreenReady(VisualElement root);

        protected virtual void OnScreenClosing()
        {
        }

        private void Start()
        {
            if (Context == null)
            {
                Debug.LogError("[UI] No game context. The screen '" + name + "' cannot start.");
                return;
            }

            _document = GetComponent<UIDocument>();
            if (_document == null)
            {
                _document = gameObject.AddComponent<UIDocument>();
            }

            if (_document.panelSettings == null)
            {
                _document.panelSettings = Context.Catalog.PanelSettings != null
                    ? Context.Catalog.PanelSettings
                    : CreateFallbackPanelSettings();
            }

            var layout = SelectLayout(Context.Catalog);
            if (layout == null)
            {
                Debug.LogError("[UI] The Game Data Catalog has no layout assigned for '" + GetType().Name + "'.");
                return;
            }

            if (_document.visualTreeAsset != layout)
            {
                _document.visualTreeAsset = layout;
            }

            OnScreenReady(_document.rootVisualElement);
        }

        private void OnDisable()
        {
            if (_document != null && _document.rootVisualElement != null)
            {
                OnScreenClosing();
            }
        }

        private static PanelSettings CreateFallbackPanelSettings()
        {
            Debug.LogWarning("[UI] Panel settings missing from the catalog; using runtime defaults.");
            var settings = ScriptableObject.CreateInstance<PanelSettings>();
            settings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            settings.referenceResolution = new Vector2Int(390, 844);
            settings.screenMatchMode = PanelScreenMatchMode.Shrink;
            return settings;
        }
    }
}
