using System;
using Nation.Core.Countries;
using Nation.Core.Models;
using Nation.Core.Signals;
using Nation.Core.Simulation;
using Nation.Core.Time;
using Nation.Core.World;

namespace Nation.Core.Session
{
    /// <summary>
    /// One running game: the world, the engine that advances it, the chosen speed and the tick schedule.
    /// Engine-free so it is testable and reusable by a server. The Unity side only feeds it elapsed time.
    /// </summary>
    public sealed class GameSession
    {
        private readonly TickScheduler _scheduler = new TickScheduler();

        public WorldState World { get; }
        public SimulationEngine Engine { get; }
        public SignalBus Signals { get; }
        public ICountryDataProvider Countries { get; }
        public GameSpeed Speed { get; private set; } = GameSpeed.Paused;

        public string PlayerCountryId => World.PlayerCountryId;
        public CountryState PlayerCountry => World.PlayerCountry;
        public CountryDefinition PlayerDefinition => Countries.Get(World.PlayerCountryId);

        private GameSession(WorldState world, SimulationEngine engine, SignalBus signals, ICountryDataProvider countries)
        {
            World = world;
            Engine = engine;
            Signals = signals;
            Countries = countries;
        }

        /// <summary>Creates a new world at the epoch containing every playable country, led by the chosen one.</summary>
        public static GameSession StartNew(string playerCountryId, int seed, ICountryDataProvider countries, SignalBus signals)
        {
            if (countries == null) throw new ArgumentNullException(nameof(countries));
            if (signals == null) throw new ArgumentNullException(nameof(signals));

            var world = WorldFactory.CreateFromDefinitions(seed, playerCountryId, countries.Playable);
            var engine = new SimulationEngine(Array.Empty<ISimulationSystem>(), signals);
            return new GameSession(world, engine, signals, countries);
        }

        public void SetSpeed(GameSpeed speed)
        {
            if (Speed == speed)
            {
                return;
            }

            Speed = speed;
            _scheduler.Reset();
            Signals.Publish(new GameSpeedChangedSignal(speed));
        }

        public void TogglePause()
        {
            SetSpeed(Speed == GameSpeed.Paused ? GameSpeed.Normal : GameSpeed.Paused);
        }

        /// <summary>Feeds elapsed real time; runs however many whole days are due. Returns the number of ticks run.</summary>
        public int AdvanceRealTime(double deltaSeconds)
        {
            var ticks = _scheduler.Advance(Speed, deltaSeconds);
            for (var i = 0; i < ticks; i++)
            {
                Engine.Tick(World);
            }

            return ticks;
        }

        public void TickOnce()
        {
            Engine.Tick(World);
        }
    }
}
