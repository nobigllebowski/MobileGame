using System;
using System.Collections.Generic;
using System.Diagnostics;
using Nation.Core.Models;
using Nation.Core.Signals;
using Nation.Core.Utilities;

namespace Nation.Core.Simulation
{
    /// <summary>
    /// Advances a world by exactly one day per Tick and runs the configured systems in order.
    /// The order of systems is the documented simulation order; it is fixed at construction.
    /// Nothing here runs per frame: the Unity side decides when to call Tick.
    /// </summary>
    public sealed class SimulationEngine
    {
        private readonly ISimulationSystem[] _systems;
        private readonly SignalBus _signals;
        private readonly Stopwatch _stopwatch = new Stopwatch();

        public IReadOnlyList<ISimulationSystem> Systems => _systems;

        /// <summary>Wall-clock duration of the most recent tick, for the performance overlay.</summary>
        public double LastTickMilliseconds { get; private set; }

        public SimulationEngine(IEnumerable<ISimulationSystem> systems, SignalBus signals)
        {
            if (systems == null)
            {
                throw new ArgumentNullException(nameof(systems));
            }

            _signals = signals ?? throw new ArgumentNullException(nameof(signals));
            _systems = new List<ISimulationSystem>(systems).ToArray();
        }

        public void Tick(WorldState world)
        {
            if (world == null)
            {
                throw new ArgumentNullException(nameof(world));
            }

            _stopwatch.Restart();

            world.CurrentDate = world.CurrentDate.AddDays(1);
            world.TickCount++;

            var random = new DeterministicRandom(DeterministicRandom.Derive(world.Seed, world.TickCount));
            var context = new TickContext(world.CurrentDate, world.TickCount, random, _signals);

            for (var i = 0; i < _systems.Length; i++)
            {
                _systems[i].Tick(world, context);
            }

            _stopwatch.Stop();
            LastTickMilliseconds = _stopwatch.Elapsed.TotalMilliseconds;

            _signals.Publish(new TickCompletedSignal(world.CurrentDate, world.TickCount, LastTickMilliseconds));
        }

        public void Tick(WorldState world, int days)
        {
            for (var i = 0; i < days; i++)
            {
                Tick(world);
            }
        }
    }
}
