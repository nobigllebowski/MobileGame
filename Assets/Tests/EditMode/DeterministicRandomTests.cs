using Nation.Core.Utilities;
using NUnit.Framework;

namespace Nation.Tests
{
    public sealed class DeterministicRandomTests
    {
        [Test]
        public void Same_Seed_Produces_Same_Sequence()
        {
            var a = new DeterministicRandom(1234);
            var b = new DeterministicRandom(1234);

            for (var i = 0; i < 100; i++)
            {
                Assert.AreEqual(a.NextUInt64(), b.NextUInt64());
            }
        }

        [Test]
        public void Different_Seeds_Diverge()
        {
            var a = new DeterministicRandom(1);
            var b = new DeterministicRandom(2);

            Assert.AreNotEqual(a.NextUInt64(), b.NextUInt64());
        }

        [Test]
        public void NextInt_Stays_Within_Range()
        {
            var random = new DeterministicRandom(99);

            for (var i = 0; i < 1000; i++)
            {
                var value = random.NextInt(-5, 5);
                Assert.IsTrue(value >= -5 && value < 5);
            }
        }

        [Test]
        public void NextDouble_Is_In_Unit_Interval()
        {
            var random = new DeterministicRandom(7);

            for (var i = 0; i < 1000; i++)
            {
                var value = random.NextDouble();
                Assert.IsTrue(value >= 0.0 && value < 1.0);
            }
        }

        [Test]
        public void Chance_Handles_Edge_Probabilities()
        {
            var random = new DeterministicRandom(5);

            Assert.IsFalse(random.Chance(0));
            Assert.IsTrue(random.Chance(1));
        }

        [Test]
        public void Zero_Seed_Is_Usable()
        {
            var random = new DeterministicRandom(0);

            Assert.AreNotEqual(0UL, random.NextUInt64());
        }
    }
}
