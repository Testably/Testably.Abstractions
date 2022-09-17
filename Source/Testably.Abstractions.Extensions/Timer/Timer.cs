using System;
using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.Timer;

internal sealed class Timer : IStoppedTimer, IRunningTimer
{
    private readonly ITimeSystem _timeSystem;
    private CancellationTokenSource? _runningCancellationTokenSource;

    public Timer(
        ITimeSystem timeSystem,
        TimeSpan interval,
        Action<CancellationToken> callback,
        Action<Exception>? onError)
    {
        _timeSystem = timeSystem;
        Interval = interval;
        Callback = callback;
        OnError = onError;
    }

    #region IRunningTimer Members

    /// <inheritdoc cref="IRunningTimer.Stop()" />
    public IStoppedTimer Stop()
    {
        _runningCancellationTokenSource?.Cancel();
        return this;
    }

    #endregion

    #region IStoppedTimer Members

    /// <inheritdoc cref="IDisposable.Dispose()" />
    public void Dispose()
    {
        Stop();
    }

    /// <inheritdoc cref="IStoppedTimer.Start(CancellationToken)" />
    public IRunningTimer Start(CancellationToken cancellationToken = default)
    {
        Stop();
        _runningCancellationTokenSource = new CancellationTokenSource();
        CancellationToken linkedToken = CreateLinkedCancellationToken(
            _runningCancellationTokenSource,
            cancellationToken);
        Task.Factory.StartNew(
            () =>
            {
                DateTime nextPlannedExecution = _timeSystem.DateTime.UtcNow;
                while (!linkedToken.IsCancellationRequested)
                {
                    nextPlannedExecution += Interval;
                    try
                    {
                        Callback(linkedToken);
                    }
                    catch (Exception e)
                    {
                        OnError?.Invoke(e);
                    }

                    TimeSpan delay = nextPlannedExecution - _timeSystem.DateTime.UtcNow;
                    if (delay < TimeSpan.Zero)
                    {
                        delay = TimeSpan.Zero;
                    }

                    _timeSystem.Task.TryDelay(delay, linkedToken);
                }
            },
            TaskCreationOptions.LongRunning);

        return this;
    }

    public TimeSpan Interval { get; }
    public Action<CancellationToken> Callback { get; }
    public Action<Exception>? OnError { get; }

    #endregion

    private static CancellationToken CreateLinkedCancellationToken(
        CancellationTokenSource internalCancellationTokenSource,
        CancellationToken externalCancellationToken)
    {
        CancellationToken internalToken = internalCancellationTokenSource.Token;
        CancellationTokenSource linkedCancellationTokenSource = CancellationTokenSource
           .CreateLinkedTokenSource(internalToken, externalCancellationToken);
        CancellationToken linkedToken = linkedCancellationTokenSource.Token;
        return linkedToken;
    }
}