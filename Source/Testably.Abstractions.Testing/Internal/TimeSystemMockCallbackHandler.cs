using System;
using System.Collections.Concurrent;

namespace Testably.Abstractions.Testing.Internal;

internal class TimeSystemMockCallbackHandler : TimeSystemMock.ICallbackHandler
{
    private readonly ConcurrentDictionary<Guid, Action<DateTime>>
        _dateTimeReadCallbacks =
            new();

    private readonly ConcurrentDictionary<Guid, Action<TimeSpan>>
        _taskDelayCallbacks =
            new();

    private readonly ConcurrentDictionary<Guid, Action<TimeSpan>>
        _threadSleepCallbacks =
            new();

    #region ICallbackHandler Members

    /// <inheritdoc cref="TimeSystemMock.ICallbackHandler.DateTimeRead(Action{DateTime})" />
    public IDisposable DateTimeRead(Action<DateTime> callback)
        => CallbackHandler.RegisterCallback(_dateTimeReadCallbacks, callback);

    /// <inheritdoc cref="TimeSystemMock.ICallbackHandler.TaskDelay(Action{TimeSpan})" />
    public IDisposable TaskDelay(Action<TimeSpan> callback)
        => CallbackHandler.RegisterCallback(_taskDelayCallbacks, callback);

    /// <inheritdoc cref="TimeSystemMock.ICallbackHandler.ThreadSleep(Action{TimeSpan})" />
    public IDisposable ThreadSleep(Action<TimeSpan> callback)
        => CallbackHandler.RegisterCallback(_threadSleepCallbacks, callback);

    #endregion

    public void InvokeDateTimeReadCallbacks(DateTime now)
        => CallbackHandler.InvokeCallbacks(_dateTimeReadCallbacks, now);

    public void InvokeTaskDelayCallbacks(TimeSpan delay)
        => CallbackHandler.InvokeCallbacks(_taskDelayCallbacks, delay);

    public void InvokeThreadSleepCallbacks(TimeSpan timeout)
        => CallbackHandler.InvokeCallbacks(_threadSleepCallbacks, timeout);
}