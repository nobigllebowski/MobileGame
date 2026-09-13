namespace Nation.Core.Models
{
    /// <summary>
    /// Dynamic state of one country. Everything here changes during play and is saved.
    /// Static facts about a country (name, capital, borders) live in definitions, not here.
    /// Phase 1 holds the six primary metrics; later phases add sub-objects (economy, budget, projects).
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
        public float Influence { get; set; }

        /// <summary>Abstract energy units per day.</summary>
        public double EnergyProduction { get; set; }

        /// <summary>Abstract energy units per day.</summary>
        public double EnergyConsumption { get; set; }

        public double EnergyBalance => EnergyProduction - EnergyConsumption;

        public CountryState()
        {
        }

        public CountryState(string id)
        {
            Id = id;
        }
    }
}
