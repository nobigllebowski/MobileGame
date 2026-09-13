using Nation.Core.Models;

namespace Nation.Core.Simulation
{
    /// <summary>
    /// One step of the per-tick simulation (population, economy, construction, ...).
    /// Systems mutate the world state and publish signals. They never touch UI or engine code.
    /// </summary>
    public interface ISimulationSystem
    {
        string Name { get; }

        void Tick(WorldState world, TickContext context);
    }
}
