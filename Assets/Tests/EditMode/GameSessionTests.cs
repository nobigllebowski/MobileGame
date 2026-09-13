using System.Collections.Generic;
using Nation.Core.Countries;
using Nation.Core.Data;
using Nation.Core.Models;
using Nation.Core.Session;
using Nation.Core.Signals;
using Nation.Core.Time;
using Nation.Core.World;
using NUnit.Framework;

namespace Nation.Tests
{
    public sealed class GameSessionTests
    {
        private static CountryCatalog Catalog() => new CountryCatalog(CountryDataParser.Parse(TestData.Countries));

        [Test]
        public void World_Is_Created_From_Definitions_At_The_Epoch()
        {
            var world = WorldFactory.CreateFromDefinitions(1, "DEU", Catalog().Playable);

            Assert.AreEqual(10, world.Countries.Count);
            Assert.AreEqual("2026-01-01", world.CurrentDate.ToIsoString());
            Assert.AreEqual(0, world.TickCount);
            Assert.AreEqual("DEU", world.PlayerCountryId);
            Assert.AreEqual(84500000L, world.PlayerCountry.Population);
            Assert.AreEqual(4.5e12, world.PlayerCountry.Gdp);
            Assert.AreEqual(5.0e11, world.PlayerCountry.Treasury);
            Assert.IsTrue(world.TryGetCountry("JPN", out var japan));
            Assert.AreEqual(92f, japan.Technology);
        }

        [Test]
        public void Player_Country_Must_Exist()
        {
            Assert.Throws<System.ArgumentException>(() => WorldFactory.CreateFromDefinitions(1, "XXX", Catalog().Playable));
            Assert.Throws<System.ArgumentException>(() => WorldFactory.Create(1, "", new List<CountryState>()));
        }

        [Test]
        public void Session_Starts_Paused_With_The_Selected_Country()
        {
            var session = GameSession.StartNew("FRA", 5, Catalog(), new SignalBus());

            Assert.AreEqual(GameSpeed.Paused, session.Speed);
            Assert.AreEqual("FRA", session.PlayerCountryId);
            Assert.AreEqual("FRA", session.PlayerDefinition.Id);
            Assert.AreEqual(session.World.PlayerCountry, session.PlayerCountry);
            Assert.AreEqual(0, session.AdvanceRealTime(10.0));
            Assert.AreEqual(0, session.World.TickCount);
        }

        [Test]
        public void Speed_Change_Publishes_Signal_Once()
        {
            var bus = new SignalBus();
            var received = new List<GameSpeed>();
            bus.Subscribe<GameSpeedChangedSignal>(s => received.Add(s.Speed));
            var session = GameSession.StartNew("DEU", 5, Catalog(), bus);

            session.SetSpeed(GameSpeed.Fast);
            session.SetSpeed(GameSpeed.Fast);
            session.TogglePause();
            session.TogglePause();

            Assert.AreEqual(new[] { GameSpeed.Fast, GameSpeed.Paused, GameSpeed.Normal }, received.ToArray());
        }

        [Test]
        public void Real_Time_Advances_Days_According_To_Speed()
        {
            var session = GameSession.StartNew("DEU", 5, Catalog(), new SignalBus());

            session.SetSpeed(GameSpeed.Normal);
            Assert.AreEqual(1, session.AdvanceRealTime(1.0));

            session.SetSpeed(GameSpeed.Slow);
            Assert.AreEqual(0, session.AdvanceRealTime(1.0));
            Assert.AreEqual(1, session.AdvanceRealTime(1.0));

            session.SetSpeed(GameSpeed.Fast);
            Assert.AreEqual(3, session.AdvanceRealTime(1.0));

            session.SetSpeed(GameSpeed.VeryFast);
            Assert.AreEqual(10, session.AdvanceRealTime(1.0));

            Assert.AreEqual(15, session.World.TickCount);
            Assert.AreEqual("2026-01-16", session.World.CurrentDate.ToIsoString());
        }

        [Test]
        public void Scheduler_Accumulates_Fractions_Across_Frames()
        {
            var scheduler = new TickScheduler();
            var ticks = 0;
            for (var frame = 0; frame < 60; frame++)
            {
                ticks += scheduler.Advance(GameSpeed.Normal, 1.0 / 60.0);
            }

            Assert.AreEqual(1, ticks);
        }

        [Test]
        public void Scheduler_Caps_Bursts_After_A_Hitch()
        {
            var scheduler = new TickScheduler();

            var ticks = scheduler.Advance(GameSpeed.VeryFast, 30.0);

            Assert.AreEqual(TickScheduler.DefaultMaxTicksPerAdvance, ticks);
            Assert.AreEqual(0, scheduler.Advance(GameSpeed.VeryFast, 0.0));
        }

        [Test]
        public void Paused_Never_Ticks_And_Drops_Accumulated_Time()
        {
            var scheduler = new TickScheduler();
            scheduler.Advance(GameSpeed.Normal, 0.9);

            Assert.AreEqual(0, scheduler.Advance(GameSpeed.Paused, 5.0));
            scheduler.Reset();
            Assert.AreEqual(0, scheduler.Advance(GameSpeed.Normal, 0.5));
        }
    }
}
