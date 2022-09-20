using System;
using System.Collections.Concurrent;

namespace Testably.Abstractions.Testing.Internal;

internal class TimeSystemMockCallbackHandler : TimeSystemMock.ICallbackHandler
{
    private readonly ConcurrentDictionary<Guid, CallbackHandler.CallbackWaiter<DateTime>>
        _dateTimeReadCallbacks =
            new();

    private readonly ConcurrentDictionary<Guid, CallbackHandler.CallbackWaiter<TimeSpan>>
        _taskDelayCallbacks =
            new();

    private readonly ConcurrentDictionary<Guid, CallbackHandler.CallbackWaiter<TimeSpan>>
        _threadSleepCallbacks =
            new();

    #region ICallbackHandler Members

    /// <inheritdoc cref="TimeSystemMock.ICallbackHandler.DateTimeRead(Action{DateTime})" />
    public IAwaitableCallback DateTimeRead(Action<DateTime>? callback = null)
        => CallbackHandler.RegisterCallback(_dateTimeReadCallbacks, callback);

    /// <inheritdoc cref="TimeSystemMock.ICallbackHandler.TaskDelay(Action{TimeSpan})" />
    public IAwaitableCallback TaskDelay(Action<TimeSpan>? callback = null)
        => CallbackHandler.RegisterCallback(_taskDelayCallbacks, callback);

    /// <inheritdoc cref="TimeSystemMock.ICallbackHandler.ThreadSleep(Action{TimeSpan})" />
    public IAwaitableCallback ThreadSleep(Action<TimeSpan>? callback = null)
        => CallbackHandler.RegisterCallback(_threadSleepCallbacks, callback);

    #endregion

    public void InvokeDateTimeReadCallbacks(DateTime now)
        => CallbackHandler.InvokeCallbacks(_dateTimeReadCallbacks, now);

    public void InvokeTaskDelayCallbacks(TimeSpan delay)
        => CallbackHandler.InvokeCallbacks(_taskDelayCallbacks, delay);

    public void InvokeThreadSleepCallbacks(TimeSpan timeout)
        => CallbackHandler.InvokeCallbacks(_threadSleepCallbacks, timeout);
}