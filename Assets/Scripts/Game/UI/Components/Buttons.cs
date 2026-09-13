using System;
using UnityEngine.UIElements;

namespace Nation.Game.UI.Components
{
    /// <summary>Button factories. All variants share the .btn base; variants only change color and size.</summary>
    public static class Buttons
    {
        public static Button Primary(string text, Action onClick) => Make(text, "btn--primary", onClick);
        public static Button Secondary(string text, Action onClick) => Make(text, "btn--secondary", onClick);
        public static Button Ghost(string text, Action onClick) => Make(text, "btn--ghost", onClick);
        public static Button Danger(string text, Action onClick) => Make(text, "btn--danger", onClick);
        public static Button Small(string text, Action onClick) => Make(text, "btn--small", onClick);

        public static Button Icon(IconKind icon, Action onClick, string extraClass = null)
        {
            var button = new Button(onClick) { text = string.Empty };
            button.AddToClassList("btn");
            button.AddToClassList("btn--icon");
            if (!string.IsNullOrEmpty(extraClass))
            {
                button.AddToClassList(extraClass);
            }

            var glyph = new IconElement(icon);
            glyph.AddToClassList("btn__icon");
            button.Add(glyph);
            button.userData = glyph;
            return button;
        }

        public static IconElement GlyphOf(Button iconButton)
        {
            return iconButton.userData as IconElement;
        }

        private static Button Make(string text, string variantClass, Action onClick)
        {
            var button = new Button(onClick) { text = text };
            button.AddToClassList("btn");
            button.AddToClassList(variantClass);
            return button;
        }
    }
}
