namespace Nation.Core.Models
{
    /// <summary>
    /// Playback speed chosen by the player. Session state, not world state: it is never part of the simulation.
    /// </summary>
    public enum GameSpeed
    {
        Paused = 0,
        Slow = 1,
        Normal = 2,
        Fast = 3,
        VeryFast = 4
    }
}
