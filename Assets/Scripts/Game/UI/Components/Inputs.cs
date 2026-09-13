using System;
using Nation.Game.UI.Core;
using UnityEngine.UIElements;

namespace Nation.Game.UI.Components
{
    /// <summary>Search box with leading glyph and a clear button. Raises Changed on every edit.</summary>
    public sealed class SearchField : VisualElement
    {
        private readonly TextField _field;
        private readonly Button _clear;

        public event Action<string> Changed;

        public string Value => _field.value;

        public SearchField(string placeholder)
        {
            AddToClassList("search");

            var glyph = new IconElement(IconKind.Search) { Color = Palette.TextMuted };
            glyph.AddToClassList("search__icon");
            Add(glyph);

            _field = new TextField();
            _field.AddToClassList("search__field");
            _field.textEdition.placeholder = placeholder;
            _field.textEdition.hidePlaceholderOnFocus = false;
            _field.RegisterValueChangedCallback(evt =>
            {
                UpdateClear();
                Changed?.Invoke(evt.newValue);
            });
            Add(_field);

            _clear = Buttons.Icon(IconKind.Close, () =>
            {
                _field.value = string.Empty;
                _field.Focus();
            }, "search__clear");
            Buttons.GlyphOf(_clear).Color = Palette.TextSecondary;
            Add(_clear);
            UpdateClear();
        }

        public void ClearText()
        {
            _field.value = string.Empty;
        }

        private void UpdateClear()
        {
            _clear.style.display = string.IsNullOrEmpty(_field.value) ? DisplayStyle.None : DisplayStyle.Flex;
        }
    }

    /// <summary>Tappable pill showing a filter's name and, when active, its chosen value.</summary>
    public sealed class FilterChip : VisualElement
    {
        private readonly Label _label;
        private readonly string _title;

        public event Action Clicked;

        public bool IsActive { get; private set; }

        public FilterChip(string title, bool showChevron = true)
        {
            _title = title;
            AddToClassList("chip");
            _label = new Label(title);
            _label.AddToClassList("chip__label");
            Add(_label);
            if (showChevron)
            {
                var glyph = new IconElement(IconKind.ChevronRight) { Color = Palette.TextMuted };
                glyph.AddToClassList("chip__chevron");
                Add(glyph);
            }

            RegisterCallback<ClickEvent>(_ => Clicked?.Invoke());
        }

        public void SetValue(string value)
        {
            IsActive = !string.IsNullOrEmpty(value);
            _label.text = IsActive ? _title + ": " + value : _title;
            EnableInClassList("chip--active", IsActive);
        }

        public void SetSelected(bool selected)
        {
            IsActive = selected;
            EnableInClassList("chip--active", selected);
        }
    }

    /// <summary>One selectable row inside a picker sheet.</summary>
    public sealed class OptionRow : VisualElement
    {
        public OptionRow(string text, bool selected, Action onClick)
        {
            AddToClassList("option-row");
            EnableInClassList("option-row--selected", selected);
            Add(Typography.Body(text, "option-row__text"));
            var check = new IconElement(IconKind.Check) { Color = Palette.Accent };
            check.AddToClassList("option-row__check");
            check.style.visibility = selected ? Visibility.Visible : Visibility.Hidden;
            Add(check);
            RegisterCallback<ClickEvent>(_ => onClick?.Invoke());
        }
    }
}
