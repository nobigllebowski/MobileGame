using System;
using System.Collections.Generic;
using Nation.Core.Countries;
using Nation.Core.Models;

namespace Nation.Core.Data
{
    /// <summary>
    /// Reads Assets/Data/Countries/countries.json into definitions. The file format is documented in the file
    /// itself; values are hand-curated approximations and are meant to be replaced by a better dataset later.
    /// </summary>
    public static class CountryDataParser
    {
        public const int SupportedSchemaVersion = 1;

        public static List<CountryDefinition> Parse(string json)
        {
            var root = JsonParser.Parse(json);
            var schema = root["schemaVersion"].AsInt(0);
            if (schema != SupportedSchemaVersion)
            {
                throw new InvalidOperationException("countries.json schema version " + schema + " is not supported (expected " + SupportedSchemaVersion + ").");
            }

            var result = new List<CountryDefinition>();
            foreach (var node in root["countries"].Items)
            {
                result.Add(ParseCountry(node));
            }

            return result;
        }

        public static CountryDefinition ParseCountry(JsonValue node)
        {
            var id = node["id"].AsString();
            if (string.IsNullOrEmpty(id))
            {
                throw new InvalidOperationException("A country entry has no id.");
            }

            var regionId = node["region"].AsString(string.Empty);
            if (!CountryEnums.TryParseRegion(regionId, out var region))
            {
                throw new InvalidOperationException("Country " + id + " has unknown region '" + regionId + "'.");
            }

            var governmentId = node["government"].AsString(string.Empty);
            if (!CountryEnums.TryParseGovernment(governmentId, out var government))
            {
                throw new InvalidOperationException("Country " + id + " has unknown government '" + governmentId + "'.");
            }

            var energy = node["energy"];
            var resources = node["resources"];
            var taxes = node["taxes"];

            return new CountryDefinition
            {
                Id = id.ToUpperInvariant(),
                Iso2 = node["iso2"].AsString(string.Empty).ToUpperInvariant(),
                Region = region,
                Government = government,
                Playable = node["playable"].AsBool(true),
                CapitalLatitude = node["capital"]["lat"].AsDouble(),
                CapitalLongitude = node["capital"]["lon"].AsDouble(),
                AreaKm2 = node["areaKm2"].AsDouble(),
                Population = node["population"].AsLong(),
                Gdp = node["gdp"].AsDouble(),
                Treasury = node["treasury"].AsDouble(),
                Technology = node["technology"].AsFloat(),
                Stability = node["stability"].AsFloat(),
                Happiness = node["happiness"].AsFloat(),
                Influence = node["influence"].AsFloat(),
                MilitaryStrength = node["military"].AsFloat(),
                EnergyProduction = energy["production"].AsDouble(),
                EnergyConsumption = energy["consumption"].AsDouble(),
                Oil = ParseBalance(resources["oil"]),
                Gas = ParseBalance(resources["gas"]),
                Food = ParseBalance(resources["food"]),
                Iron = ParseBalance(resources["iron"]),
                DefaultTaxes = new TaxPolicy(taxes["income"].AsFloat(), taxes["business"].AsFloat(), taxes["import"].AsFloat()),
                StrengthKeys = node["strengths"].AsStringArray(),
                ChallengeKeys = node["challenges"].AsStringArray(),
                Flag = ParseFlag(node["flag"])
            };
        }

        private static ResourceBalance ParseBalance(JsonValue node)
        {
            return new ResourceBalance(node["production"].AsDouble(), node["consumption"].AsDouble());
        }

        private static FlagSpec ParseFlag(JsonValue node)
        {
            var layers = new List<FlagLayer>();
            foreach (var layerNode in node["layers"].Items)
            {
                var kindId = layerNode["kind"].AsString(string.Empty);
                if (!TryParseKind(kindId, out var kind))
                {
                    throw new InvalidOperationException("Unknown flag layer kind '" + kindId + "'.");
                }

                var layer = new FlagLayer
                {
                    Kind = kind,
                    Colors = layerNode["colors"].AsStringArray(),
                    Count = layerNode["count"].AsInt(0),
                    X = layerNode["x"].AsFloat(0.5f),
                    Y = layerNode["y"].AsFloat(0.5f),
                    Size = layerNode["size"].AsFloat(0.25f),
                    Width = layerNode["width"].AsFloat(0f),
                    Height = layerNode["height"].AsFloat(0f),
                    Thickness = layerNode["thickness"].AsFloat(0.1f),
                    Points = layerNode["points"].AsInt(5)
                };

                if (layerNode.Has("color"))
                {
                    layer.Colors = new[] { layerNode["color"].AsString("#FFFFFF") };
                }

                layers.Add(layer);
            }

            return new FlagSpec(layers);
        }

        private static bool TryParseKind(string id, out FlagLayerKind kind)
        {
            switch (id)
            {
                case "fill": kind = FlagLayerKind.Fill; return true;
                case "hstripes": kind = FlagLayerKind.HorizontalStripes; return true;
                case "vstripes": kind = FlagLayerKind.VerticalStripes; return true;
                case "disc": kind = FlagLayerKind.Disc; return true;
                case "ring": kind = FlagLayerKind.Ring; return true;
                case "star": kind = FlagLayerKind.Star; return true;
                case "canton": kind = FlagLayerKind.Canton; return true;
                case "cross": kind = FlagLayerKind.Cross; return true;
                case "saltire": kind = FlagLayerKind.Saltire; return true;
                case "diamond": kind = FlagLayerKind.Diamond; return true;
                default: kind = FlagLayerKind.Fill; return false;
            }
        }
    }
}
