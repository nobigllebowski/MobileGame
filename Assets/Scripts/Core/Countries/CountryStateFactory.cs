using Nation.Core.Models;

namespace Nation.Core.Countries
{
    /// <summary>Seeds a fresh dynamic state from a definition's starting values.</summary>
    public static class CountryStateFactory
    {
        public static CountryState Create(CountryDefinition definition)
        {
            return new CountryState(definition.Id)
            {
                Population = definition.Population,
                Gdp = definition.Gdp,
                Treasury = definition.Treasury,
                Happiness = definition.Happiness,
                Stability = definition.Stability,
                Influence = definition.Influence,
                Technology = definition.Technology,
                MilitaryStrength = definition.MilitaryStrength,
                EnergyProduction = definition.EnergyProduction,
                EnergyConsumption = definition.EnergyConsumption,
                Oil = definition.Oil,
                Gas = definition.Gas,
                Food = definition.Food,
                Iron = definition.Iron,
                Taxes = definition.DefaultTaxes
            };
        }
    }
}
