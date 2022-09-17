using System.Threading;

namespace Testably.Abstractions;

/// <summary>
///     A pending timer.<br />
///     When <see cref="IStoppedTimer.Start(CancellationToken)" /> is called, executes <see cref="ITimer.Callback" /> in a
///     regular <see cref="ITimer.Interval" />.
/// </summary>
public interface IStoppedTimer : ITimer
{
    /// <summary>
    ///     Starts the timer.
    /// </summary>
    IRunningTimer Start(CancellationToken cancellationToken = default);
}