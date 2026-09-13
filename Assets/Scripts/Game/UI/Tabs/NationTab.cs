using System.Collections.Generic;
using Nation.Core.Countries;
using Nation.Core.Nation;
using Nation.Game.Bootstrap;
using Nation.Game.UI.Components;
using Nation.Game.UI.Core;
using Nation.Game.UI.Screens;
using UnityEngine.UIElements;

namespace Nation.Game.UI.Tabs
{
    /// <summary>The player's command center: identity, primary and secondary metrics, status and alerts.</summary>
    public sealed class NationTab : GameTab
    {
        private Label _date;
        private StatCard _treasury;
        private StatCard _gdp;
        private StatCard _population;
        private StatCard _stability;
        private StatCard _happiness;
        private StatCard _influence;
        private Label _energyValue;
        private readonly List<StatusRow> _statusRows = new List<StatusRow>();
        private VisualElement _alerts;

        public NationTab(GameContext context, GameShellScreen shell) : base(context, shell, "tab-nation")
        {
        }

        protected override void Build()
        {
            var country = Session.PlayerCountry;
            var definition = Session.PlayerDefinition;

            var scroll = Scroll("nation");
            Add(scroll);

            var identity = new VisualElement();
            identity.AddToClassList("identity");
            identity.Add(new FlagElement(definition.Flag, "flag--hud"));
            var text = new VisualElement();
            text.AddToClassList("identity__text");
            text.Add(Typography.Title(Loc.Get(definition.NameKey), "identity__name"));
            text.Add(Typography.Caption(Loc.Get(definition.GovernmentKey), "identity__meta"));
            _date = Typography.Caption(Format.Date(Session.World.CurrentDate), "identity__date");
            text.Add(_date);
            identity.Add(text);
            var menu = Buttons.Icon(IconKind.Menu, Shell.RequestMainMenu, "identity__menu");
            Buttons.GlyphOf(menu).Color = Palette.TextSecondary;
            identity.Add(menu);
            scroll.Add(identity);

            scroll.Add(Typography.SectionTitle(Loc.Get("nation.primary"), "section-title"));
            var grid = new VisualElement();
            grid.AddToClassList("grid-2");
            _treasury = new StatCard(Loc.Get("stat.treasury"), string.Empty).Variant("stat-card--gold");
            _gdp = new StatCard(Loc.Get("stat.gdp"), string.Empty);
            _population = new StatCard(Loc.Get("stat.population"), string.Empty);
            _stability = new StatCard(Loc.Get("stat.stability"), string.Empty);
            _happiness = new StatCard(Loc.Get("stat.happiness"), string.Empty);
            _influence = new StatCard(Loc.Get("stat.influence"), string.Empty);
            grid.Add(_treasury);
            grid.Add(_gdp);
            grid.Add(_population);
            grid.Add(_stability);
            grid.Add(_happiness);
            grid.Add(_influence);
            scroll.Add(grid);

            var secondary = new InfoCard(Loc.Get("nation.secondary"));
            secondary.AddRow(Loc.Get("stat.technology"), Loc.Get(CountryTiers.Key(CountryTiers.Technology(country.Technology))) + "  ·  " + Format.Index(country.Technology));
            _energyValue = secondary.AddRow(Loc.Get("stat.energy"), string.Empty);
            secondary.AddRow(Loc.Get("stat.resources"), ResourceSummary());
            secondary.AddRow(Loc.Get("stat.government"), Loc.Get(definition.GovernmentKey));
            secondary.AddRow(Loc.Get("stat.capital"), Loc.Get(definition.CapitalKey));
            secondary.AddRow(Loc.Get("stat.area"), Format.Area(definition.AreaKm2));
            scroll.Add(secondary);

            var status = new InfoCard(Loc.Get("nation.status"));
            foreach (var entry in NationAssessment.Status(country))
            {
                var row = new StatusRow(Loc.Get(entry.TopicKey), Loc.Get(entry.ValueKey), entry.Level);
                _statusRows.Add(row);
                status.Content.Add(row);
            }
            scroll.Add(status);

            scroll.Add(Typography.SectionTitle(Loc.Get("nation.alerts"), "section-title"));
            _alerts = new VisualElement();
            _alerts.AddToClassList("alerts");
            scroll.Add(_alerts);

            var spacer = new VisualElement();
            spacer.AddToClassList("tab__spacer");
            scroll.Add(spacer);

            Refresh();
        }

        public override void OnShow()
        {
            if (IsBuilt)
            {
                Refresh();
            }
        }

        public override void OnTick()
        {
            Refresh();
        }

        private void Refresh()
        {
            var country = Session.PlayerCountry;
            _date.text = Format.Date(Session.World.CurrentDate);
            _treasury.SetValue(Format.Money(country.Treasury));
            _gdp.SetValue(Format.Money(country.Gdp));
            _population.SetValue(Format.Population(country.Population));
            _stability.SetValue(Format.Percent(country.Stability));
            _happiness.SetValue(Format.Percent(country.Happiness));
            _influence.SetValue(Format.Index(country.Influence));

            var sufficiency = country.EnergySelfSufficiency;
            _energyValue.text = Format.Percent(sufficiency * 100.0) + "  " + Loc.Get("stat.energy_self_sufficiency");
            _energyValue.EnableInClassList("text--danger", sufficiency < 0.9);
            _energyValue.EnableInClassList("text--warning", sufficiency >= 0.9 && sufficiency < 1.0);
            _energyValue.EnableInClassList("text--positive", sufficiency >= 1.0);

            var status = NationAssessment.Status(country);
            for (var i = 0; i < status.Count && i < _statusRows.Count; i++)
            {
                _statusRows[i].Set(Loc.Get(status[i].ValueKey), status[i].Level);
            }

            RebuildAlerts(NationAssessment.Alerts(country, Session.PlayerDefinition));
        }

        private int _alertSignature = -1;

        private void RebuildAlerts(List<NationAlert> alerts)
        {
            var signature = alerts.Count;
            foreach (var alert in alerts)
            {
                signature = signature * 31 + alert.TitleKey.GetHashCode() + (int)alert.Level;
            }

            if (signature == _alertSignature)
            {
                return;
            }

            _alertSignature = signature;
            _alerts.Clear();
            if (alerts.Count == 0)
            {
                _alerts.Add(new EmptyState(IconKind.Check, Loc.Get("nation.alerts.none.title"), Loc.Get("nation.alerts.none.body")));
                return;
            }

            foreach (var alert in alerts)
            {
                _alerts.Add(new AlertCard(Loc.Get(alert.TitleKey), Loc.Get(alert.BodyKey), alert.Level));
            }
        }

        private string ResourceSummary()
        {
            var country = Session.PlayerCountry;
            return Loc.Get("resource.oil.short") + " " + Format.Percent(country.Oil.SelfSufficiency * 100) + "  ·  "
                   + Loc.Get("resource.gas.short") + " " + Format.Percent(country.Gas.SelfSufficiency * 100) + "  ·  "
                   + Loc.Get("resource.food.short") + " " + Format.Percent(country.Food.SelfSufficiency * 100) + "  ·  "
                   + Loc.Get("resource.iron.short") + " " + Format.Percent(country.Iron.SelfSufficiency * 100);
        }
    }
}
