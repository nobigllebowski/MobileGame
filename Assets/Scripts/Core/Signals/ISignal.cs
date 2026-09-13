namespace Nation.Core.Signals
{
    /// <summary>
    /// Marker for in-memory notifications published by the simulation and services.
    /// Signals describe something that happened. They are not player-facing content;
    /// see EventDefinition in a later phase for that.
    /// </summary>
    public interface ISignal
    {
    }
}
