namespace Nation.Core.Models
{
    /// <summary>Daily production and consumption of one resource in abstract units.</summary>
    public struct ResourceBalance
    {
        public double Production { get; set; }
        public double Consumption { get; set; }

        public ResourceBalance(double production, double consumption)
        {
            Production = production;
            Consumption = consumption;
        }

        public double Net => Production - Consumption;

        /// <summary>Production divided by consumption. 1.0 means self-sufficient; above 1 means an exportable surplus.</summary>
        public double SelfSufficiency => Consumption <= 0 ? (Production > 0 ? 2.0 : 1.0) : Production / Consumption;
    }
}
