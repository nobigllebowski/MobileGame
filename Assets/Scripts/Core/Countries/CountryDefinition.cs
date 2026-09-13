using Nation.Core.Models;

namespace Nation.Core.Countries
{
    /// <summary>
    /// Immutable, versioned description of a country: identity, geography and starting values.
    /// Starting values seed a CountryState when a world is created and are never read again during play.
    /// All player-facing text is a localization key derived from the ISO3 id.
    /// </summary>
    public sealed class CountryDefinition
    {
        public string Id { get; set; }
        public string Iso2 { get; set; }
        public CountryRegion Region { get; set; }
        public GovernmentType Government { get; set; }
        public bool Playable { get; set; } = true;

        public double CapitalLatitude { get; set; }
        public double CapitalLongitude { get; set; }
        public double AreaKm2 { get; set; }

        public long Population { get; set; }
        public double Gdp { get; set; }
        public double Treasury { get; set; }
        public float Technology { get; set; }
        public float Stability { get; set; }
        public float Happiness { get; set; }
        public float Influence { get; set; }
        public float MilitaryStrength { get; set; }

        public double EnergyProduction { get; set; }
        public double EnergyConsumption { get; set; }
        public ResourceBalance Oil { get; set; }
        public ResourceBalance Gas { get; set; }
        public ResourceBalance Food { get; set; }
        public ResourceBalance Iron { get; set; }
        public TaxPolicy DefaultTaxes { get; set; }

        public string[] StrengthKeys { get; set; } = new string[0];
        public string[] ChallengeKeys { get; set; } = new string[0];
        public FlagSpec Flag { get; set; } = FlagSpec.Empty;

        public string NameKey => "country." + Id;
        public string CapitalKey => "capital." + Id;
        public string GovernmentKey => CountryEnums.Key(Government);
        public string RegionKey => CountryEnums.Key(Region);

        public double GdpPerCapita => Population <= 0 ? 0 : Gdp / Population;
        public double EnergySelfSufficiency => EnergyConsumption <= 0 ? 1.0 : EnergyProduction / EnergyConsumption;
    }
}
