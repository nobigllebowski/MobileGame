using System;

namespace Nation.Core.Countries
{
    public enum CountryDifficulty
    {
        Easy,
        Normal,
        Hard,
        VeryHard
    }

    /// <summary>
    /// Derives difficulty from the starting data instead of hand-assigning it. A country is easier to lead when it
    /// is rich per citizen, stable, content, technologically capable and self-sufficient in energy and food.
    /// Weights are the tuning surface; thresholds split the 0-100 score into four bands.
    /// </summary>
    public static class CountryDifficultyCalculator
    {
        public const float WealthWeight = 0.30f;
        public const float TechnologyWeight = 0.15f;
        public const float StabilityWeight = 0.20f;
        public const float HappinessWeight = 0.10f;
        public const float EnergyWeight = 0.125f;
        public const float FoodWeight = 0.125f;

        public const float EasyThreshold = 75f;
        public const float NormalThreshold = 62f;
        public const float HardThreshold = 50f;

        public static float Score(CountryDefinition country)
        {
            var wealth = WealthScore(country.GdpPerCapita);
            var energy = SufficiencyScore(country.EnergySelfSufficiency);
            var food = SufficiencyScore(country.Food.SelfSufficiency);

            var score = wealth * WealthWeight
                        + country.Technology * TechnologyWeight
                        + country.Stability * StabilityWeight
                        + country.Happiness * HappinessWeight
                        + energy * EnergyWeight
                        + food * FoodWeight;

            return Clamp(score, 0f, 100f);
        }

        public static CountryDifficulty Calculate(CountryDefinition country)
        {
            var score = Score(country);
            if (score >= EasyThreshold) return CountryDifficulty.Easy;
            if (score >= NormalThreshold) return CountryDifficulty.Normal;
            if (score >= HardThreshold) return CountryDifficulty.Hard;
            return CountryDifficulty.VeryHard;
        }

        public static string Key(CountryDifficulty difficulty)
        {
            switch (difficulty)
            {
                case CountryDifficulty.Easy: return "difficulty.easy";
                case CountryDifficulty.Normal: return "difficulty.normal";
                case CountryDifficulty.Hard: return "difficulty.hard";
                default: return "difficulty.very_hard";
            }
        }

        /// <summary>Logarithmic: $1k per person scores 0, $100k per person scores 100.</summary>
        private static float WealthScore(double gdpPerCapita)
        {
            if (gdpPerCapita <= 1000)
            {
                return 0f;
            }

            var value = Math.Log(gdpPerCapita / 1000.0) / Math.Log(100.0) * 100.0;
            return Clamp((float)value, 0f, 100f);
        }

        /// <summary>Self-sufficiency of 1.0 scores 50; every 10 percent surplus or deficit moves it by 15 points.</summary>
        private static float SufficiencyScore(double ratio)
        {
            return Clamp((float)(50.0 + (ratio - 1.0) * 150.0), 0f, 100f);
        }

        private static float Clamp(float value, float min, float max)
        {
            return value < min ? min : value > max ? max : value;
        }
    }
}
