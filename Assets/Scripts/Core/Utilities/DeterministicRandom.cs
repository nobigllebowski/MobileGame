using System;

namespace Nation.Core.Utilities
{
    /// <summary>
    /// Seeded xorshift64* generator. Produces the same sequence on Mono, IL2CPP and .NET,
    /// which System.Random does not guarantee. Used for every random decision in the simulation.
    /// </summary>
    public sealed class DeterministicRandom
    {
        private ulong _state;

        public DeterministicRandom(ulong seed)
        {
            // A zero state would lock xorshift at zero forever; SplitMix64 also spreads clustered seeds.
            _state = SplitMix64(seed == 0 ? 0x9E3779B97F4A7C15UL : seed);
        }

        public DeterministicRandom(int seed) : this(unchecked((ulong)(uint)seed))
        {
        }

        /// <summary>Derives an independent stream for a given tick so a world's randomness never depends on tick history.</summary>
        public static ulong Derive(int worldSeed, long streamIndex)
        {
            return SplitMix64(unchecked((ulong)(uint)worldSeed * 0xBF58476D1CE4E5B9UL + (ulong)streamIndex));
        }

        public ulong NextUInt64()
        {
            var x = _state;
            x ^= x >> 12;
            x ^= x << 25;
            x ^= x >> 27;
            _state = x;
            return unchecked(x * 0x2545F4914F6CDD1DUL);
        }

        /// <summary>Uniform double in [0, 1).</summary>
        public double NextDouble()
        {
            return (NextUInt64() >> 11) * (1.0 / 9007199254740992.0);
        }

        /// <summary>Uniform integer in [minInclusive, maxExclusive).</summary>
        public int NextInt(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive)
            {
                throw new ArgumentOutOfRangeException(nameof(maxExclusive), "maxExclusive must be greater than minInclusive.");
            }

            var range = (ulong)((long)maxExclusive - minInclusive);
            return (int)(minInclusive + (long)(NextUInt64() % range));
        }

        /// <summary>Uniform double in [minInclusive, maxInclusive].</summary>
        public double NextRange(double minInclusive, double maxInclusive)
        {
            return minInclusive + (maxInclusive - minInclusive) * NextDouble();
        }

        /// <summary>True with the given probability in [0, 1].</summary>
        public bool Chance(double probability)
        {
            if (probability <= 0)
            {
                return false;
            }

            if (probability >= 1)
            {
                return true;
            }

            return NextDouble() < probability;
        }

        private static ulong SplitMix64(ulong value)
        {
            unchecked
            {
                value += 0x9E3779B97F4A7C15UL;
                value = (value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL;
                value = (value ^ (value >> 27)) * 0x94D049BB133111EBUL;
                return value ^ (value >> 31);
            }
        }
    }
}
