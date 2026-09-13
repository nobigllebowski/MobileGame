using Nation.Core.Models;
using Nation.Core.Utilities;

namespace Nation.Core.Economy
{
    /// <summary>
    /// Derived economic figures for the Economy screen. PLACEHOLDER MODEL: every value is a simple function of
    /// the current state so the screen reads coherently. The economy simulation phase replaces the formulas
    /// with the sector model; the UI keeps reading this type.
    /// </summary>
    public readonly struct EconomyOverview
    {
        public double Gdp { get; }
        /// <summary>Annual growth in percent.</summary>
        public double GdpGrowth { get; }
        public double Treasury { get; }
        public double AnnualRevenue { get; }
        public double AnnualExpenses { get; }
        public double AnnualBalance => AnnualRevenue - AnnualExpenses;
        public TaxPolicy Taxes { get; }
        /// <summary>Twelve monthly GDP values ending at the current GDP, for the chart.</summary>
        public double[] GdpHistory { get; }

        public EconomyOverview(double gdp, double gdpGrowth, double treasury, double annualRevenue, double annualExpenses, TaxPolicy taxes, double[] gdpHistory)
        {
            Gdp = gdp;
            GdpGrowth = gdpGrowth;
            Treasury = treasury;
            AnnualRevenue = annualRevenue;
            AnnualExpenses = annualExpenses;
            Taxes = taxes;
            GdpHistory = gdpHistory;
        }

        public static EconomyOverview From(CountryState state, int worldSeed)
        {
            var taxes = state.Taxes;
            var revenueShare = 0.18 + taxes.IncomeTax * 0.004 + taxes.BusinessTax * 0.002 + taxes.ImportTax * 0.002;
            var revenue = state.Gdp * revenueShare;
            var pressure = 1.04 - (state.Stability - 50) / 1000.0;
            var expenses = revenue * pressure;

            var growth = state.Technology / 100.0 * 3.2
                         + (state.EnergySelfSufficiency - 1.0) * 2.0
                         + (state.Happiness - 60) / 40.0
                         - (state.GdpPerCapita > 40000 ? 0.8 : 0.0);
            if (growth < -3) growth = -3;
            if (growth > 8) growth = 8;

            var history = new double[12];
            var random = new DeterministicRandom(DeterministicRandom.Derive(worldSeed, 7001));
            var monthly = growth / 100.0 / 12.0;
            var value = state.Gdp;
            for (var i = 11; i >= 0; i--)
            {
                history[i] = value;
                var noise = (random.NextDouble() - 0.5) * 0.004;
                value /= 1.0 + monthly + noise;
            }

            return new EconomyOverview(state.Gdp, growth, state.Treasury, revenue, expenses, taxes, history);
        }
    }
}
