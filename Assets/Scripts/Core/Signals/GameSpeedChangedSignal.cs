using Nation.Core.Models;

namespace Nation.Core.Signals
{
    public readonly struct GameSpeedChangedSignal : ISignal
    {
        public GameSpeed Speed { get; }

        public GameSpeedChangedSignal(GameSpeed speed)
        {
            Speed = speed;
        }
    }
}
