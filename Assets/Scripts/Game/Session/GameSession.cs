using System;
using Nation.Core.Models;
using Nation.Core.Signals;
using Nation.Core.Simulation;
using Nation.Core.World;

namespace Nation.Game.Session
{
    /// <summary>
    /// One running game: a world state plus the engine that advances it.
    /// The time service (Phase 6) will call TickOnce on a schedule; for now the UI calls it directly.
    /// </summary>
    public sealed class GameSession
    {
        public WorldState World { get; }
        public SimulationEngine Engine { get; }
        public GameSpeed Speed { get; set; } = GameSpeed.Paused;

        public CountryState PlayerCountry => World.PlayerCountry;

        private GameSession(WorldState world, SimulationEngine engine)
        {
            World = world ?? throw new ArgumentNullException(nameof(world));
            Engine = engine ?? throw new ArgumentNullException(nameof(engine));
        }

        public static GameSession StartNew(string playerCountryId, int seed, SignalBus signals)
        {
            var world = WorldFactory.Create(seed, playerCountryId, PlaceholderCountries.Create());
            var engine = new SimulationEngine(Array.Empty<ISimulationSystem>(), signals);
            return new GameSession(world, engine);
        }

        public void TickOnce()
        {
            Engine.Tick(World);
        }
    }
}
