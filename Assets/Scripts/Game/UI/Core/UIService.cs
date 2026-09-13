using Nation.Core.Localization;
using Nation.Game.Config;
using Nation.Game.UI.Components;
using Nation.Game.UI.Formatting;
using UnityEngine;
using UnityEngine.UIElements;

namespace Nation.Game.UI.Core
{
    /// <summary>
    /// Owns the single persistent UI document and its layers, bottom to top:
    /// screens, bottom sheets, modal dialogs, loading overlay, toasts. All inside the safe area.
    /// </summary>
    public sealed class UIService
    {
        public const int ReferenceWidth = 390;
        public const int ReferenceHeight = 844;

        private readonly VisualElement _loading;

        public VisualElement Root { get; }
        public SafeAreaElement SafeArea { get; }
        public ScreenStack Screens { get; }
        public SheetLayer Sheets { get; }
        public ModalLayer Modals { get; }
        public ToastLayer Toasts { get; }
        public UiFormat Format { get; }
        public ILocalizationService Loc { get; }

        public UIService(UIDocument document, GameDataCatalog catalog, ILocalizationService localization)
        {
            Loc = localization;
            Format = new UiFormat(localization);

            Root = document.rootVisualElement;
            Root.name = "app-root";
            Root.AddToClassList("app-root");
            if (catalog.Theme != null)
            {
                Root.styleSheets.Add(catalog.Theme);
            }
            else
            {
                Debug.LogError("[UI] No theme stylesheet assigned in the Game Data Catalog; the UI will be unstyled.");
            }

            SafeArea = new SafeAreaElement();
            Root.Add(SafeArea);

            var screenLayer = Layer("screen-layer");
            var sheetLayer = Layer("sheet-layer");
            var modalLayer = Layer("modal-layer");
            _loading = Layer("loading-layer");
            var toastLayer = Layer("toast-layer");

            Screens = new ScreenStack(screenLayer);
            Sheets = new SheetLayer(sheetLayer);
            Modals = new ModalLayer(modalLayer);
            Toasts = new ToastLayer(toastLayer);

            _loading.style.display = DisplayStyle.None;
            _loading.Add(new LoadingIndicator());
        }

        public void Toast(string message, ToastKind kind = ToastKind.Info)
        {
            Toasts.Show(message, kind);
        }

        public void ToastKey(string key, ToastKind kind = ToastKind.Info)
        {
            Toasts.Show(Loc.Get(key), kind);
        }

        public void SetLoading(bool visible)
        {
            _loading.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private VisualElement Layer(string layerName)
        {
            var layer = new VisualElement { name = layerName };
            layer.AddToClassList("layer");
            layer.AddToClassList(layerName);
            SafeArea.Add(layer);
            return layer;
        }

        public static PanelSettings CreateFallbackPanelSettings()
        {
            Debug.LogWarning("[UI] Panel settings missing from the catalog; using runtime defaults.");
            var settings = ScriptableObject.CreateInstance<PanelSettings>();
            settings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            settings.referenceResolution = new Vector2Int(ReferenceWidth, ReferenceHeight);
            settings.screenMatchMode = PanelScreenMatchMode.Shrink;
            return settings;
        }
    }
}
