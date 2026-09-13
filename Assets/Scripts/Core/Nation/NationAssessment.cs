using System.Collections.Generic;
using Nation.Core.Countries;
using Nation.Core.Models;

namespace Nation.Core.Nation
{
    public enum StatusLevel
    {
        Positive,
        Neutral,
        Warning,
        Danger
    }

    /// <summary>One row of the NATIONAL STATUS section: a topic and a level, both as localization keys.</summary>
    public readonly struct StatusEntry
    {
        public string TopicKey { get; }
        public string ValueKey { get; }
        public StatusLevel Level { get; }

        public StatusEntry(string topicKey, string valueKey, StatusLevel level)
        {
            TopicKey = topicKey;
            ValueKey = valueKey;
            Level = level;
        }
    }

    /// <summary>One alert card generated from state.</summary>
    public readonly struct NationAlert
    {
        public string TitleKey { get; }
        public string BodyKey { get; }
        public StatusLevel Level { get; }

        public NationAlert(string titleKey, string bodyKey, StatusLevel level)
        {
            TitleKey = titleKey;
            BodyKey = bodyKey;
            Level = level;
        }
    }

    /// <summary>
    /// Reads a country's state and produces the quick status rows and alerts shown on the Nation screen.
    /// Pure functions over state: no UI, no engine, easy to test and to refine as the simulation grows.
    /// </summary>
    public static class NationAssessment
    {
        public static List<StatusEntry> Status(CountryState state)
        {
            var result = new List<StatusEntry>(4);

            var perCapita = state.GdpPerCapita;
            if (perCapita >= 35000) result.Add(new StatusEntry("status.topic.economy", "status.strong", StatusLevel.Positive));
            else if (perCapita >= 12000) result.Add(new StatusEntry("status.topic.economy", "status.stable", StatusLevel.Neutral));
            else result.Add(new StatusEntry("status.topic.economy", "status.developing", StatusLevel.Warning));

            var energy = state.EnergySelfSufficiency;
            if (energy >= 1.0) result.Add(new StatusEntry("status.topic.energy", "status.secure", StatusLevel.Positive));
            else if (energy >= 0.9) result.Add(new StatusEntry("status.topic.energy", "status.warning", StatusLevel.Warning));
            else result.Add(new StatusEntry("status.topic.energy", "status.critical", StatusLevel.Danger));

            if (state.Happiness >= 65) result.Add(new StatusEntry("status.topic.population", "status.stable", StatusLevel.Positive));
            else if (state.Happiness >= 50) result.Add(new StatusEntry("status.topic.population", "status.strained", StatusLevel.Warning));
            else result.Add(new StatusEntry("status.topic.population", "status.unrest", StatusLevel.Danger));

            if (state.Influence >= 80) result.Add(new StatusEntry("status.topic.diplomacy", "status.strong", StatusLevel.Positive));
            else if (state.Influence >= 60) result.Add(new StatusEntry("status.topic.diplomacy", "status.neutral", StatusLevel.Neutral));
            else result.Add(new StatusEntry("status.topic.diplomacy", "status.isolated", StatusLevel.Warning));

            return result;
        }

        public static List<NationAlert> Alerts(CountryState state, CountryDefinition definition)
        {
            var result = new List<NationAlert>(4);

            var energy = state.EnergySelfSufficiency;
            if (energy < 0.9)
            {
                result.Add(new NationAlert("alert.energy_dependency.title", "alert.energy_dependency.body", StatusLevel.Danger));
            }
            else if (energy < 1.0)
            {
                result.Add(new NationAlert("alert.energy_dependency.title", "alert.energy_dependency.body", StatusLevel.Warning));
            }

            if (state.Stability < 60)
            {
                result.Add(new NationAlert("alert.stability_low.title", "alert.stability_low.body", StatusLevel.Danger));
            }

            if (state.Food.SelfSufficiency < 1.0)
            {
                result.Add(new NationAlert("alert.food_imports.title", "alert.food_imports.body", StatusLevel.Warning));
            }

            if (definition != null && HasTrait(definition.ChallengeKeys, "trait.aging_population"))
            {
                result.Add(new NationAlert("alert.population_growth.title", "alert.population_growth.body", StatusLevel.Warning));
            }

            if (state.Treasury < state.Gdp * 0.05)
            {
                result.Add(new NationAlert("alert.low_reserves.title", "alert.low_reserves.body", StatusLevel.Warning));
            }

            if (state.Influence >= 70)
            {
                result.Add(new NationAlert("alert.trade_opportunities.title", "alert.trade_opportunities.body", StatusLevel.Positive));
            }

            if (state.Iron.SelfSufficiency >= 1.5 || state.Oil.SelfSufficiency >= 1.3)
            {
                result.Add(new NationAlert("alert.export_surplus.title", "alert.export_surplus.body", StatusLevel.Positive));
            }

            return result;
        }

        private static bool HasTrait(string[] keys, string trait)
        {
            if (keys == null)
            {
                return false;
            }

            for (var i = 0; i < keys.Length; i++)
            {
                if (keys[i] == trait)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
