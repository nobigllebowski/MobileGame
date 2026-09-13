using Nation.Core.Economy;
using Nation.Core.Nation;
using Nation.Game.Bootstrap;
using Nation.Game.UI.Components;
using Nation.Game.UI.Core;
using Nation.Game.UI.Screens;
using UnityEngine.UIElements;

namespace Nation.Game.UI.Tabs
{
    /// <summary>Economy dashboard over the placeholder EconomyOverview model.</summary>
    public sealed class EconomyTab : GameTab
    {
        private StatCard _gdp;
        private StatCard _growth;
        private StatCard _treasury;
        private StatCard _revenue;
        private StatCard _expenses;
        private StatCard _balance;
        private ChartElement _chart;
        private GaugeBar _income;
        private GaugeBar _business;
        private GaugeBar _import;

        public EconomyTab(GameContext context, GameShellScreen shell) : base(context, shell, "tab-economy")
        {
        }

        protected override void Build()
        {
            var scroll = Scroll("economy");
            Add(scroll);

            scroll.Add(TabHeader(Loc.Get("economy.title"), Loc.Get("economy.subtitle")));

            var grid = new VisualElement();
            grid.AddToClassList("grid-2");
            _gdp = new StatCard(Loc.Get("stat.gdp"), string.Empty);
            _growth = new StatCard(Loc.Get("economy.growth"), string.Empty);
            _treasury = new StatCard(Loc.Get("stat.treasury"), string.Empty).Variant("stat-card--gold");
            _revenue = new StatCard(Loc.Get("economy.revenue"), string.Empty, Loc.Get("economy.per_year"));
            _expenses = new StatCard(Loc.Get("economy.expenses"), string.Empty, Loc.Get("economy.per_year"));
            _balance = new StatCard(Loc.Get("economy.balance"), string.Empty, Loc.Get("economy.per_year"));
            grid.Add(_gdp);
            grid.Add(_growth);
            grid.Add(_treasury);
            grid.Add(_revenue);
            grid.Add(_expenses);
            grid.Add(_balance);
            scroll.Add(grid);

            var chartCard = new InfoCard(Loc.Get("economy.chart_title"), Loc.Get("economy.chart_subtitle"));
            _chart = new ChartElement(ChartElement.Mode.Line);
            chartCard.Content.Add(_chart);
            scroll.Add(chartCard);

            var taxes = new InfoCard(Loc.Get("economy.taxation"), Loc.Get("economy.taxation_hint"));
            _income = new GaugeBar(Loc.Get("tax.income"), string.Empty, 0f);
            _business = new GaugeBar(Loc.Get("tax.business"), string.Empty, 0f);
            _import = new GaugeBar(Loc.Get("tax.import"), string.Empty, 0f);
            taxes.Content.Add(_income);
            taxes.Content.Add(_business);
            taxes.Content.Add(_import);
            scroll.Add(taxes);

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
            var overview = EconomyOverview.From(Session.PlayerCountry, Session.World.Seed);
            _gdp.SetValue(Format.Money(overview.Gdp));
            _growth.SetValue(Format.SignedPercent(overview.GdpGrowth));
            _growth.SetNote(Loc.Get(overview.GdpGrowth >= 0 ? "economy.growth_positive" : "economy.growth_negative"),
                overview.GdpGrowth >= 0 ? StatusLevel.Positive : StatusLevel.Danger);
            _treasury.SetValue(Format.Money(overview.Treasury));
            _revenue.SetValue(Format.Money(overview.AnnualRevenue));
            _expenses.SetValue(Format.Money(overview.AnnualExpenses));
            _balance.SetValue(Format.SignedMoney(overview.AnnualBalance));
            _balance.SetNote(Loc.Get(overview.AnnualBalance >= 0 ? "economy.surplus" : "economy.deficit"),
                overview.AnnualBalance >= 0 ? StatusLevel.Positive : StatusLevel.Warning);
            _chart.SetValues(overview.GdpHistory, overview.GdpGrowth >= 0 ? Palette.Positive : Palette.Danger);
            _income.Set(Format.Percent(overview.Taxes.IncomeTax), overview.Taxes.IncomeTax / 60f);
            _business.Set(Format.Percent(overview.Taxes.BusinessTax), overview.Taxes.BusinessTax / 60f);
            _import.Set(Format.Percent(overview.Taxes.ImportTax), overview.Taxes.ImportTax / 30f);
        }
    }
}
