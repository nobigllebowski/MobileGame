using System.Collections.Generic;

namespace Nation.Core.Countries
{
    /// <summary>
    /// Source of static country definitions. The MVP provider reads bundled JSON; a later provider may load an
    /// updated dataset, behind the same interface, without touching gameplay code.
    /// </summary>
    public interface ICountryDataProvider
    {
        IReadOnlyList<CountryDefinition> All { get; }

        IReadOnlyList<CountryDefinition> Playable { get; }

        bool TryGet(string iso3, out CountryDefinition definition);

        CountryDefinition Get(string iso3);
    }
}
