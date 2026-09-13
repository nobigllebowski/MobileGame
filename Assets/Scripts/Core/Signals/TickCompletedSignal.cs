using Nation.Core.Models;

namespace Nation.Core.Signals
{
    /// <summary>Published once at the end of every simulation tick.</summary>
    public readonly struct TickCompletedSignal : ISignal
    {
        public GameDate Date { get; }
        public long TickCount { get; }
        public double TickMilliseconds { get; }

        public TickCompletedSignal(GameDate date, long tickCount, double tickMilliseconds)
        {
            Date = date;
            TickCount = tickCount;
            TickMilliseconds = tickMilliseconds;
        }
    }
}
