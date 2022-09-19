using System;
using System.Threading;
using Testably.Abstractions;

namespace Examples.TimeSystem.Timer;

/// <summary>
///     This example illustrates usage of a timer to execute a synchronization repeatedly.
/// </summary>
public sealed class TimerExample : IDisposable
{
    private readonly Action<Exception>? _errorHandler;
    private IRunningTimer? _runningTimer;
    private readonly ISynchronization _synchronization;
    private readonly ITimeSystem _timeSystem;

    public TimerExample(
        ITimeSystem timeSystem,
        ISynchronization synchronization,
        Action<Exception>? errorHandler = null)
    {
        _timeSystem = timeSystem;
        _synchronization = synchronization;
        _errorHandler = errorHandler;
    }

    #region IDisposable Members

    /// <inheritdoc />
    public void Dispose()
    {
        _runningTimer?.Dispose();
    }

    #endregion

    public void Start(CancellationToken cancellationToken)
    {
        _runningTimer = _timeSystem.CreateTimer(
                _synchronization.Interval,
                _synchronization.Execute,
                _errorHandler)
           .Start(cancellationToken);
    }

    public interface ISynchronization
    {
        public TimeSpan Interval { get; }
        public void Execute(CancellationToken cancellationToken);
    }
}