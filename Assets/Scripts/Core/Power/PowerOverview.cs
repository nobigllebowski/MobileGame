using System.Collections.Generic;
using Nation.Core.Models;

namespace Nation.Core.Power
{
    /// <summary>
    /// Strategic power ratings for the Power screen, each 0 to 100. PLACEHOLDER MODEL relative to the other
    /// countries in the world; the strategic power phase replaces the formulas.
    /// </summary>
    public readonly struct PowerOverview
    {
        public float NationalPower { get; }
        public float MilitaryReadiness { get; }
        public float EconomicPower { get; }
        public float TechnologicalPower { get; }
        public float GlobalInfluence { get; }

        public PowerOverview(float nationalPower, float militaryReadiness, float economicPower, float technologicalPower, float globalInfluence)
        {
            NationalPower = nationalPower;
            MilitaryReadiness = militaryReadiness;
            EconomicPower = economicPower;
            TechnologicalPower = technologicalPower;
            GlobalInfluence = globalInfluence;
        }

        public static PowerOverview From(CountryState state, IReadOnlyList<CountryState> world)
        {
            double maxGdp = 0;
            for (var i = 0; i < world.Count; i++)
            {
                if (world[i].Gdp > maxGdp)
                {
                    maxGdp = world[i].Gdp;
                }
            }

            var economic = maxGdp <= 0 ? 0f : (float)(state.Gdp / maxGdp * 100.0);
            var military = state.MilitaryStrength;
            var technological = state.Technology;
            var influence = state.Influence;
            var national = economic * 0.35f + military * 0.25f + technological * 0.2f + influence * 0.2f;

            return new PowerOverview(national, military, economic, technological, influence);
        }
    }
}
