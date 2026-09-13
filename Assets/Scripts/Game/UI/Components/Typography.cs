using UnityEngine.UIElements;

namespace Nation.Game.UI.Components
{
    /// <summary>Label factories for the type hierarchy. Classes are defined in Theme.uss.</summary>
    public static class Typography
    {
        public static Label Eyebrow(string text, string extraClass = null) => Make(text, "type-eyebrow", extraClass);
        public static Label Display(string text, string extraClass = null) => Make(text, "type-display", extraClass);
        public static Label Title(string text, string extraClass = null) => Make(text, "type-title", extraClass);
        public static Label Heading(string text, string extraClass = null) => Make(text, "type-heading", extraClass);
        public static Label Subtitle(string text, string extraClass = null) => Make(text, "type-subtitle", extraClass);
        public static Label Body(string text, string extraClass = null) => Make(text, "type-body", extraClass);
        public static Label Caption(string text, string extraClass = null) => Make(text, "type-caption", extraClass);
        public static Label Value(string text, string extraClass = null) => Make(text, "type-value", extraClass);
        public static Label SectionTitle(string text, string extraClass = null) => Make(text, "type-section", extraClass);

        private static Label Make(string text, string typeClass, string extraClass)
        {
            var label = new Label(text);
            label.AddToClassList(typeClass);
            if (!string.IsNullOrEmpty(extraClass))
            {
                label.AddToClassList(extraClass);
            }

            return label;
        }
    }
}
