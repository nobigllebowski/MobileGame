using System.Collections.Generic;
using Nation.Core.Models;
using Nation.Core.Signals;
using Nation.Core.Simulation;
using Nation.Core.World;
using NUnit.Framework;

namespace Nation.Tests
{
    public sealed class SimulationEngineTests
    {
        private sealed class RecordingSystem : ISimulationSystem
        {
            private readonly List<string> _log;

            public RecordingSystem(string name, List<string> log)
            {
                Name = name;
                _log = log;
            }

            public string Name { get; }

            public GameDate LastDate { get; private set; }

            public void Tick(WorldState world, TickContext context)
            {
                _log.Add(Name);
                LastDate = context.Date;
            }
        }

        private static WorldState CreateWorld()
        {
            return WorldFactory.Create(42, "DEU", new[] { new CountryState("DEU") { Population = 1 } });
        }

        [Test]
        public void Tick_Advances_Exactly_One_Day()
        {
            var world = CreateWorld();
            var engine = new SimulationEngine(new ISimulationSystem[0], new SignalBus());

            engine.Tick(world);

            Assert.AreEqual("2026-01-02", world.CurrentDate.ToIsoString());
            Assert.AreEqual(1, world.TickCount);

            engine.Tick(world, 30);
            Assert.AreEqual("2026-02-01", world.CurrentDate.ToIsoString());
            Assert.AreEqual(31, world.TickCount);
        }

        [Test]
        public void Systems_Run_In_Declared_Order_With_The_New_Date()
        {
            var log = new List<string>();
            var first = new RecordingSystem("first", log);
            var second = new RecordingSystem("second", log);
            var engine = new SimulationEngine(new ISimulationSystem[] { first, second }, new SignalBus());
            var world = CreateWorld();

            engine.Tick(world);

            Assert.AreEqual(new[] { "first", "second" }, log.ToArray());
            Assert.AreEqual(world.CurrentDate, first.LastDate);
        }

        [Test]
        public void TickCompleted_Is_Published_Once_Per_Tick()
        {
            var bus = new SignalBus();
            var received = new List<TickCompletedSignal>();
            bus.Subscribe<TickCompletedSignal>(received.Add);
            var engine = new SimulationEngine(new ISimulationSystem[0], bus);
            var world = CreateWorld();

            engine.Tick(world);
            engine.Tick(world);

            Assert.AreEqual(2, received.Count);
            Assert.AreEqual(1, received[0].TickCount);
            Assert.AreEqual(2, received[1].TickCount);
            Assert.AreEqual(world.CurrentDate, received[1].Date);
        }

        [Test]
        public void Per_Tick_Random_Is_Deterministic_For_Same_Seed()
        {
            var samplesA = new List<double>();
            var samplesB = new List<double>();
            var systemA = new SamplingSystem(samplesA);
            var systemB = new SamplingSystem(samplesB);

            var engineA = new SimulationEngine(new ISimulationSystem[] { systemA }, new SignalBus());
            var engineB = new SimulationEngine(new ISimulationSystem[] { systemB }, new SignalBus());

            engineA.Tick(CreateWorld(), 5);
            engineB.Tick(CreateWorld(), 5);

            Assert.AreEqual(samplesA, samplesB);
        }

        private sealed class SamplingSystem : ISimulationSystem
        {
            private readonly List<double> _samples;

            public SamplingSystem(List<double> samples)
            {
                _samples = samples;
            }

            public string Name => "sampling";

            public void Tick(WorldState world, TickContext context)
            {
                _samples.Add(context.Random.NextDouble());
            }
        }
    }
}
