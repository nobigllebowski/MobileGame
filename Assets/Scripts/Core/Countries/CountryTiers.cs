namespace Nation.Core.Countries
{
    public enum PopulationTier { Small, Medium, Large, Huge }
    public enum EconomyTier { Emerging, Developed, Strong, Superpower }
    public enum TechnologyTier { Developing, Medium, High, Advanced }
    public enum InfluenceTier { Low, Medium, High }

    /// <summary>Data-driven classification thresholds shared by filters, cards and previews.</summary>
    public static class CountryTiers
    {
        public static PopulationTier Population(long population)
        {
            if (population >= 500000000L) return PopulationTier.Huge;
            if (population >= 100000000L) return PopulationTier.Large;
            if (population >= 20000000L) return PopulationTier.Medium;
            return PopulationTier.Small;
        }

        public static EconomyTier Economy(double gdp)
        {
            if (gdp >= 10e12) return EconomyTier.Superpower;
            if (gdp >= 3.5e12) return EconomyTier.Strong;
            if (gdp >= 1.5e12) return EconomyTier.Developed;
            return EconomyTier.Emerging;
        }

        public static TechnologyTier Technology(float technology)
        {
            if (technology >= 90) return TechnologyTier.Advanced;
            if (technology >= 80) return TechnologyTier.High;
            if (technology >= 65) return TechnologyTier.Medium;
            return TechnologyTier.Developing;
        }

        public static InfluenceTier Influence(float influence)
        {
            if (influence >= 75) return InfluenceTier.High;
            if (influence >= 60) return InfluenceTier.Medium;
            return InfluenceTier.Low;
        }

        public static string Key(PopulationTier tier)
        {
            switch (tier)
            {
                case PopulationTier.Small: return "tier.population.small";
                case PopulationTier.Medium: return "tier.population.medium";
                case PopulationTier.Large: return "tier.population.large";
                default: return "tier.population.huge";
            }
        }

        public static string Key(EconomyTier tier)
        {
            switch (tier)
            {
                case EconomyTier.Emerging: return "tier.economy.emerging";
                case EconomyTier.Developed: return "tier.economy.developed";
                case EconomyTier.Strong: return "tier.economy.strong";
                default: return "tier.economy.superpower";
            }
        }

        public static string Key(TechnologyTier tier)
        {
            switch (tier)
            {
                case TechnologyTier.Developing: return "tier.technology.developing";
                case TechnologyTier.Medium: return "tier.technology.medium";
                case TechnologyTier.High: return "tier.technology.high";
                default: return "tier.technology.advanced";
            }
        }

        public static string Key(InfluenceTier tier)
        {
            switch (tier)
            {
                case InfluenceTier.Low: return "tier.influence.low";
                case InfluenceTier.Medium: return "tier.influence.medium";
                default: return "tier.influence.high";
            }
        }
    }
}
