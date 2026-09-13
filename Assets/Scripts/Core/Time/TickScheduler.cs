using System;
using Nation.Core.Models;

namespace Nation.Core.Time
{
    /// <summary>
    /// Converts elapsed real time at a chosen speed into a whole number of simulation days to run.
    /// Fractions carry over between calls, so NORMAL yields exactly one tick per second on average regardless
    /// of frame rate. A per-call cap keeps a long hitch from stalling the frame with a burst of ticks.
    /// </summary>
    public sealed class TickScheduler
    {
        public const int DefaultMaxTicksPerAdvance = 12;

        private double _accumulatedDays;

        public int MaxTicksPerAdvance { get; set; } = DefaultMaxTicksPerAdvance;

        public static double DaysPerSecond(GameSpeed speed)
        {
            switch (speed)
            {
                case GameSpeed.Slow: return 0.5;
                case GameSpeed.Normal: return 1.0;
                case GameSpeed.Fast: return 3.0;
                case GameSpeed.VeryFast: return 10.0;
                default: return 0.0;
            }
        }

        /// <summary>Returns how many ticks are due after deltaSeconds of real time at the given speed.</summary>
        public int Advance(GameSpeed speed, double deltaSeconds)
        {
            if (speed == GameSpeed.Paused || deltaSeconds <= 0)
            {
                return 0;
            }

            _accumulatedDays += deltaSeconds * DaysPerSecond(speed);
            var ticks = (int)Math.Floor(_accumulatedDays);
            if (ticks <= 0)
            {
                return 0;
            }

            if (ticks > MaxTicksPerAdvance)
            {
                ticks = MaxTicksPerAdvance;
                _accumulatedDays = 0;
            }
            else
            {
                _accumulatedDays -= ticks;
            }

            return ticks;
        }

        public void Reset()
        {
            _accumulatedDays = 0;
        }
    }
}
