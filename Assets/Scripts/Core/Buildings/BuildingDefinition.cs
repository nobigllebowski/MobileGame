using System;
using System.Collections.Generic;
using Nation.Core.Data;

namespace Nation.Core.Buildings
{
    public enum BuildingCategory
    {
        Energy,
        Industry,
        Infrastructure,
        Housing,
        Agriculture
    }

    public enum BuildingEffectKind
    {
        Energy,
        Industry,
        Infrastructure,
        Housing,
        Food
    }

    /// <summary>Static description of a constructible project. Construction logic arrives in the construction phase.</summary>
    public sealed class BuildingDefinition
    {
        public string Id { get; set; }
        public BuildingCategory Category { get; set; }
        public double Cost { get; set; }
        public int DurationDays { get; set; }
        public double AnnualMaintenance { get; set; }
        public BuildingEffectKind EffectKind { get; set; }
        public double EffectValue { get; set; }
        public string EffectUnitKey { get; set; }

        public string NameKey => "building." + Id;
        public string DescriptionKey => "building." + Id + ".description";

        public static string CategoryKey(BuildingCategory category)
        {
            switch (category)
            {
                case BuildingCategory.Energy: return "build.category.energy";
                case BuildingCategory.Industry: return "build.category.industry";
                case BuildingCategory.Infrastructure: return "build.category.infrastructure";
                case BuildingCategory.Housing: return "build.category.housing";
                default: return "build.category.agriculture";
            }
        }

        public static string EffectKey(BuildingEffectKind kind)
        {
            switch (kind)
            {
                case BuildingEffectKind.Energy: return "build.effect.energy";
                case BuildingEffectKind.Industry: return "build.effect.industry";
                case BuildingEffectKind.Infrastructure: return "build.effect.infrastructure";
                case BuildingEffectKind.Housing: return "build.effect.housing";
                default: return "build.effect.food";
            }
        }
    }

    public static class BuildingDataParser
    {
        public static List<BuildingDefinition> Parse(string json)
        {
            var root = JsonParser.Parse(json);
            var result = new List<BuildingDefinition>();
            foreach (var node in root["buildings"].Items)
            {
                var id = node["id"].AsString();
                if (string.IsNullOrEmpty(id))
                {
                    throw new InvalidOperationException("A building entry has no id.");
                }

                result.Add(new BuildingDefinition
                {
                    Id = id,
                    Category = ParseCategory(node["category"].AsString(string.Empty), id),
                    Cost = node["cost"].AsDouble(),
                    DurationDays = node["durationDays"].AsInt(),
                    AnnualMaintenance = node["maintenance"].AsDouble(),
                    EffectKind = ParseEffect(node["effect"]["kind"].AsString(string.Empty), id),
                    EffectValue = node["effect"]["value"].AsDouble(),
                    EffectUnitKey = node["effect"]["unitKey"].AsString(string.Empty)
                });
            }

            return result;
        }

        private static BuildingCategory ParseCategory(string id, string building)
        {
            switch (id)
            {
                case "energy": return BuildingCategory.Energy;
                case "industry": return BuildingCategory.Industry;
                case "infrastructure": return BuildingCategory.Infrastructure;
                case "housing": return BuildingCategory.Housing;
                case "agriculture": return BuildingCategory.Agriculture;
                default: throw new InvalidOperationException("Building " + building + " has unknown category '" + id + "'.");
            }
        }

        private static BuildingEffectKind ParseEffect(string id, string building)
        {
            switch (id)
            {
                case "energy": return BuildingEffectKind.Energy;
                case "industry": return BuildingEffectKind.Industry;
                case "infrastructure": return BuildingEffectKind.Infrastructure;
                case "housing": return BuildingEffectKind.Housing;
                case "food": return BuildingEffectKind.Food;
                default: throw new InvalidOperationException("Building " + building + " has unknown effect '" + id + "'.");
            }
        }
    }
}
