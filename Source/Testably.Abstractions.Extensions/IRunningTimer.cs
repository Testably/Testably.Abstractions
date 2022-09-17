namespace Testably.Abstractions;

/// <summary>
///     A running timer that is executing <see cref="ITimer.Callback" /> in a regular <see cref="ITimer.Interval" />.
/// </summary>
public interface IRunningTimer : ITimer
{
    /// <summary>
    ///     Stops the current timer.
    /// </summary>
    IStoppedTimer Stop();
}