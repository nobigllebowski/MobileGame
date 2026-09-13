using System;
using Nation.Core.Nation;
using Nation.Game.UI.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Nation.Game.UI.Components
{
    /// <summary>Metric tile: small label on top, large value, optional footnote. Variants: default, gold, danger, positive.</summary>
    public sealed class StatCard : VisualElement
    {
        private readonly Label _label;
        private readonly Label _value;
        private readonly Label _note;

        public StatCard(string label, string value, string note = null)
        {
            AddToClassList("stat-card");

            var box = new VisualElement();
            box.AddToClassList("card");
            box.AddToClassList("stat-card__box");
            Add(box);

            _label = Typography.Caption(label, "stat-card__label");
            _value = Typography.Value(value, "stat-card__value");
            _note = Typography.Caption(note ?? string.Empty, "stat-card__note");
            _note.style.display = string.IsNullOrEmpty(note) ? DisplayStyle.None : DisplayStyle.Flex;

            box.Add(_label);
            box.Add(_value);
            box.Add(_note);
        }

        public StatCard Variant(string variantClass)
        {
            AddToClassList(variantClass);
            return this;
        }

        public void SetValue(string value)
        {
            if (_value.text != value)
            {
                _value.text = value;
            }
        }

        public void SetNote(string note, StatusLevel? level = null)
        {
            _note.text = note ?? string.Empty;
            _note.style.display = string.IsNullOrEmpty(note) ? DisplayStyle.None : DisplayStyle.Flex;
            _note.RemoveFromClassList("text--positive");
            _note.RemoveFromClassList("text--danger");
            _note.RemoveFromClassList("text--warning");
            if (level == StatusLevel.Positive) _note.AddToClassList("text--positive");
            else if (level == StatusLevel.Danger) _note.AddToClassList("text--danger");
            else if (level == StatusLevel.Warning) _note.AddToClassList("text--warning");
        }
    }

    /// <summary>Titled container for related rows or free content.</summary>
    public sealed class InfoCard : VisualElement
    {
        public VisualElement Content { get; }

        public InfoCard(string title, string subtitle = null)
        {
            AddToClassList("card");
            AddToClassList("info-card");

            if (!string.IsNullOrEmpty(title))
            {
                var header = new VisualElement();
                header.AddToClassList("info-card__header");
                header.Add(Typography.SectionTitle(title));
                if (!string.IsNullOrEmpty(subtitle))
                {
                    header.Add(Typography.Caption(subtitle, "info-card__subtitle"));
                }

                Add(header);
            }

            Content = new VisualElement();
            Content.AddToClassList("info-card__content");
            Add(Content);
        }

        public Label AddRow(string label, string value, StatusLevel? level = null)
        {
            var row = new VisualElement();
            row.AddToClassList("info-row");
            row.Add(Typography.Caption(label, "info-row__label"));
            var valueLabel = Typography.Body(value, "info-row__value");
            if (level == StatusLevel.Positive) valueLabel.AddToClassList("text--positive");
            else if (level == StatusLevel.Danger) valueLabel.AddToClassList("text--danger");
            else if (level == StatusLevel.Warning) valueLabel.AddToClassList("text--warning");
            row.Add(valueLabel);
            Content.Add(row);
            return valueLabel;
        }

        public void AddBullet(string text, IconKind icon, Color color)
        {
            var row = new VisualElement();
            row.AddToClassList("bullet-row");
            var glyph = new IconElement(icon) { Color = color };
            glyph.AddToClassList("bullet-row__icon");
            row.Add(glyph);
            row.Add(Typography.Body(text, "bullet-row__text"));
            Content.Add(row);
        }
    }

    /// <summary>Small pill with a colored level: STABLE, WARNING, NORMAL, HARD...</summary>
    public sealed class Badge : VisualElement
    {
        private readonly Label _label;

        public Badge(string text, StatusLevel level = StatusLevel.Neutral)
        {
            AddToClassList("badge");
            _label = new Label(text);
            _label.AddToClassList("badge__text");
            Add(_label);
            SetLevel(level);
        }

        public void SetText(string text)
        {
            _label.text = text;
        }

        public void SetLevel(StatusLevel level)
        {
            RemoveFromClassList("badge--positive");
            RemoveFromClassList("badge--neutral");
            RemoveFromClassList("badge--warning");
            RemoveFromClassList("badge--danger");
            AddToClassList("badge--" + level.ToString().ToLowerInvariant());
        }

        public static string LevelClass(StatusLevel level) => "badge--" + level.ToString().ToLowerInvariant();
    }

    /// <summary>Topic on the left, status badge on the right. Used by NATIONAL STATUS.</summary>
    public sealed class StatusRow : VisualElement
    {
        private readonly Badge _badge;

        public StatusRow(string topic, string status, StatusLevel level)
        {
            AddToClassList("status-row");
            Add(Typography.Body(topic, "status-row__topic"));
            _badge = new Badge(status, level);
            Add(_badge);
        }

        public void Set(string status, StatusLevel level)
        {
            _badge.SetText(status);
            _badge.SetLevel(level);
        }
    }

    /// <summary>Alert card with a colored edge, title and body.</summary>
    public sealed class AlertCard : VisualElement
    {
        public AlertCard(string title, string body, StatusLevel level)
        {
            AddToClassList("card");
            AddToClassList("alert-card");
            AddToClassList("alert-card--" + level.ToString().ToLowerInvariant());

            var icon = new IconElement(level == StatusLevel.Positive ? IconKind.Check : IconKind.Alert) { Color = ColorFor(level) };
            icon.AddToClassList("alert-card__icon");
            Add(icon);

            var text = new VisualElement();
            text.AddToClassList("alert-card__text");
            text.Add(Typography.Body(title, "alert-card__title"));
            text.Add(Typography.Caption(body, "alert-card__body"));
            Add(text);
        }

        public static Color ColorFor(StatusLevel level)
        {
            switch (level)
            {
                case StatusLevel.Positive: return Palette.Positive;
                case StatusLevel.Warning: return Palette.Warning;
                case StatusLevel.Danger: return Palette.Danger;
                default: return Palette.Accent;
            }
        }
    }

    /// <summary>Horizontal bar with label and value, for ratings and rates.</summary>
    public sealed class GaugeBar : VisualElement
    {
        private readonly VisualElement _fill;
        private readonly Label _value;

        public GaugeBar(string label, string value, float fraction, string variantClass = null)
        {
            AddToClassList("gauge");
            if (!string.IsNullOrEmpty(variantClass))
            {
                AddToClassList(variantClass);
            }

            var head = new VisualElement();
            head.AddToClassList("gauge__head");
            head.Add(Typography.Caption(label, "gauge__label"));
            _value = Typography.Body(value, "gauge__value");
            head.Add(_value);
            Add(head);

            var track = new VisualElement();
            track.AddToClassList("gauge__track");
            _fill = new VisualElement();
            _fill.AddToClassList("gauge__fill");
            track.Add(_fill);
            Add(track);

            Set(value, fraction);
        }

        public void Set(string value, float fraction)
        {
            _value.text = value;
            _fill.style.width = new Length(Mathf.Clamp01(fraction) * 100f, LengthUnit.Percent);
        }
    }

    /// <summary>Illustrated empty state with an optional action.</summary>
    public sealed class EmptyState : VisualElement
    {
        public EmptyState(IconKind icon, string title, string body, string actionLabel = null, Action action = null)
        {
            AddToClassList("empty-state");
            var glyph = new IconElement(icon) { Color = Palette.TextMuted };
            glyph.AddToClassList("empty-state__icon");
            Add(glyph);
            Add(Typography.Heading(title, "empty-state__title"));
            Add(Typography.Body(body, "empty-state__body"));
            if (!string.IsNullOrEmpty(actionLabel))
            {
                Add(Buttons.Secondary(actionLabel, action));
            }
        }
    }

    /// <summary>Rotating arc spinner drawn with Painter2D.</summary>
    public sealed class LoadingIndicator : VisualElement
    {
        private float _angle;
        private IVisualElementScheduledItem _animation;

        public LoadingIndicator()
        {
            AddToClassList("loading-indicator");
            pickingMode = PickingMode.Ignore;
            generateVisualContent += OnGenerate;
            RegisterCallback<AttachToPanelEvent>(_ =>
            {
                _animation?.Pause();
                _animation = schedule.Execute(state =>
                {
                    _angle = (_angle + state.deltaTime * 0.36f) % 360f;
                    MarkDirtyRepaint();
                }).Every(16);
            });
            RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                _animation?.Pause();
                _animation = null;
            });
        }

        private void OnGenerate(MeshGenerationContext context)
        {
            var rect = contentRect;
            var radius = Mathf.Min(rect.width, rect.height) * 0.4f;
            if (radius <= 0)
            {
                return;
            }

            var center = new Vector2(rect.width * 0.5f, rect.height * 0.5f);
            var p = context.painter2D;
            p.lineWidth = 3f;
            p.lineCap = LineCap.Round;

            p.strokeColor = Palette.WithAlpha(Palette.Accent, 0.18f);
            p.BeginPath();
            p.Arc(center, radius, Angle.Degrees(0), Angle.Degrees(360));
            p.Stroke();

            p.strokeColor = Palette.Accent;
            p.BeginPath();
            p.Arc(center, radius, Angle.Degrees(_angle), Angle.Degrees(_angle + 90f));
            p.Stroke();
        }
    }
}
