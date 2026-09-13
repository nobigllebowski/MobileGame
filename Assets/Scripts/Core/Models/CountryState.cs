namespace Nation.Core.Models
{
    /// <summary>
    /// Dynamic state of one country. Everything here changes during play and is saved.
    /// Static facts about a country (name, capital, borders, flag) live in CountryDefinition.
    /// </summary>
    public sealed class CountryState
    {
        /// <summary>ISO 3166-1 alpha-3 code, for example "DEU". Stable identifier across saves and data updates.</summary>
        public string Id { get; set; }

        public long Population { get; set; }

        /// <summary>Annual gross domestic product in US dollars.</summary>
        public double Gdp { get; set; }

        /// <summary>Government cash on hand in US dollars.</summary>
        public double Treasury { get; set; }

        /// <summary>0 to 100.</summary>
        public float Happiness { get; set; }

        /// <summary>0 to 100.</summary>
        public float Stability { get; set; }

        /// <summary>0 to 100.</summary>
        public float Influence { get; set; }

        /// <summary>0 to 100.</summary>
        public float Technology { get; set; }

        /// <summary>0 to 100. Abstract strategic military capability.</summary>
        public float MilitaryStrength { get; set; }

        /// <summary>Abstract energy units per day.</summary>
        public double EnergyProduction { get; set; }

        /// <summary>Abstract energy units per day.</summary>
        public double EnergyConsumption { get; set; }

        public ResourceBalance Oil { get; set; }
        public ResourceBalance Gas { get; set; }
        public ResourceBalance Food { get; set; }
        public ResourceBalance Iron { get; set; }

        public TaxPolicy Taxes { get; set; }

        public double EnergyBalance => EnergyProduction - EnergyConsumption;

        public double EnergySelfSufficiency => EnergyConsumption <= 0 ? 1.0 : EnergyProduction / EnergyConsumption;

        public double GdpPerCapita => Population <= 0 ? 0 : Gdp / Population;

        public CountryState()
        {
        }

        public CountryState(string id)
        {
            Id = id;
        }
    }
}
