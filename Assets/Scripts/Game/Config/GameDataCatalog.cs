using UnityEngine;
using UnityEngine.UIElements;

namespace Nation.Game.Config
{
    /// <summary>
    /// The one asset the bootstrap needs. It points at every static data file and UI layout the game loads,
    /// so nothing is looked up by string path at runtime and no scene needs manual wiring.
    /// Lives in a Resources folder so it can be found before the first scene loads.
    /// </summary>
    [CreateAssetMenu(fileName = "GameDataCatalog", menuName = "Nation/Game Data Catalog")]
    public sealed class GameDataCatalog : ScriptableObject
    {
        public const string ResourcePath = "GameDataCatalog";

        [Header("UI")]
        [SerializeField] private PanelSettings panelSettings;
        [SerializeField] private VisualTreeAsset mainMenuLayout;
        [SerializeField] private VisualTreeAsset worldPlaceholderLayout;

        [Header("Localization")]
        [Tooltip("One JSON table per locale. The first table's locale is the default.")]
        [SerializeField] private TextAsset[] localizationTables;

        public PanelSettings PanelSettings => panelSettings;
        public VisualTreeAsset MainMenuLayout => mainMenuLayout;
        public VisualTreeAsset WorldPlaceholderLayout => worldPlaceholderLayout;
        public TextAsset[] LocalizationTables => localizationTables;
    }
}
