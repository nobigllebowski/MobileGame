using System;
using Nation.Core.Localization;
using Nation.Game.UI.Components;
using UnityEngine.UIElements;

namespace Nation.Game.UI.Core
{
    /// <summary>Declarative description of a dialog: title, body, and one to three actions.</summary>
    public sealed class ModalDialog
    {
        public enum ActionStyle { Primary, Secondary, Danger }

        public sealed class ActionSpec
        {
            public string Label;
            public ActionStyle Style;
            public Action Callback;
        }

        public string Title;
        public string Body;
        public bool DismissOnScrim = true;
        public readonly System.Collections.Generic.List<ActionSpec> Actions = new System.Collections.Generic.List<ActionSpec>();

        public ModalDialog WithTitle(string title) { Title = title; return this; }
        public ModalDialog WithBody(string body) { Body = body; return this; }
        public ModalDialog Locked() { DismissOnScrim = false; return this; }

        public ModalDialog AddAction(string label, ActionStyle style, Action callback)
        {
            Actions.Add(new ActionSpec { Label = label, Style = style, Callback = callback });
            return this;
        }
    }

    /// <summary>Hosts one dialog at a time above every screen, with scrim fade and card scale-in.</summary>
    public sealed class ModalLayer
    {
        private const long AnimationMs = 220;
        private const string VisibleClass = "modal-layer--visible";

        private readonly VisualElement _layer;
        private readonly VisualElement _scrim;
        private readonly VisualElement _host;
        private VisualElement _card;
        private bool _dismissOnScrim;

        public bool IsOpen => _card != null;

        public ModalLayer(VisualElement layer)
        {
            _layer = layer;
            _layer.style.display = DisplayStyle.None;
            _layer.pickingMode = PickingMode.Ignore;

            _scrim = new VisualElement { name = "modal-scrim" };
            _scrim.AddToClassList("modal-scrim");
            _scrim.RegisterCallback<ClickEvent>(_ =>
            {
                if (_dismissOnScrim)
                {
                    Dismiss();
                }
            });
            _layer.Add(_scrim);

            _host = new VisualElement { name = "modal-host" };
            _host.AddToClassList("modal-host");
            _host.pickingMode = PickingMode.Ignore;
            _layer.Add(_host);
        }

        public void Show(ModalDialog dialog, ILocalizationService loc)
        {
            if (IsOpen)
            {
                Dismiss();
            }

            _dismissOnScrim = dialog.DismissOnScrim;
            _card = new VisualElement { name = "modal-card" };
            _card.AddToClassList("modal-card");

            if (!string.IsNullOrEmpty(dialog.Title))
            {
                _card.Add(Typography.Heading(dialog.Title, "modal-card__title"));
            }

            if (!string.IsNullOrEmpty(dialog.Body))
            {
                _card.Add(Typography.Body(dialog.Body, "modal-card__body"));
            }

            var actions = new VisualElement();
            actions.AddToClassList("modal-card__actions");
            foreach (var spec in dialog.Actions)
            {
                var captured = spec;
                Button button;
                switch (captured.Style)
                {
                    case ModalDialog.ActionStyle.Primary:
                        button = Buttons.Primary(captured.Label, () => Invoke(captured));
                        break;
                    case ModalDialog.ActionStyle.Danger:
                        button = Buttons.Danger(captured.Label, () => Invoke(captured));
                        break;
                    default:
                        button = Buttons.Secondary(captured.Label, () => Invoke(captured));
                        break;
                }

                actions.Add(button);
            }

            _card.Add(actions);
            _host.Add(_card);

            _layer.pickingMode = PickingMode.Position;
            _layer.style.display = DisplayStyle.Flex;
            _layer.schedule.Execute(() => _layer.AddToClassList(VisibleClass));
        }

        public void Show(ModalDialog dialog)
        {
            Show(dialog, null);
        }

        public void Dismiss()
        {
            if (_card == null)
            {
                return;
            }

            var card = _card;
            _card = null;
            _layer.RemoveFromClassList(VisibleClass);
            _layer.pickingMode = PickingMode.Ignore;
            _layer.schedule.Execute(() =>
            {
                card.RemoveFromHierarchy();
                if (_card == null)
                {
                    _layer.style.display = DisplayStyle.None;
                }
            }).StartingIn(AnimationMs);
        }

        private void Invoke(ModalDialog.ActionSpec spec)
        {
            Dismiss();
            spec.Callback?.Invoke();
        }
    }
}
