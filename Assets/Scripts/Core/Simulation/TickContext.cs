using Nation.Core.Models;
using Nation.Core.Signals;
using Nation.Core.Utilities;

namespace Nation.Core.Simulation
{
    /// <summary>Per-tick services handed to every simulation system.</summary>
    public readonly struct TickContext
    {
        public GameDate Date { get; }
        public long TickCount { get; }
        public DeterministicRandom Random { get; }
        public SignalBus Signals { get; }

        public TickContext(GameDate date, long tickCount, DeterministicRandom random, SignalBus signals)
        {
            Date = date;
            TickCount = tickCount;
            Random = random;
            Signals = signals;
        }
    }
}
