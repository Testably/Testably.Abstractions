using System;

namespace Testably.Abstractions.Testing.Internal;

internal class TimeSystemMockCallbackHandler : TimeSystemMock.ICallbackHandler
{
    private readonly Notification.INotificationFactory<DateTime>
        _dateTimeReadCallbacks = Notification.CreateFactory<DateTime>();

    private readonly Notification.INotificationFactory<TimeSpan>
        _taskDelayCallbacks = Notification.CreateFactory<TimeSpan>();

    private readonly Notification.INotificationFactory<TimeSpan>
        _threadSleepCallbacks = Notification.CreateFactory<TimeSpan>();

    #region ICallbackHandler Members

    /// <inheritdoc cref="TimeSystemMock.ICallbackHandler.DateTimeRead(Action{DateTime})" />
    public Notification.IAwaitableCallback<DateTime> DateTimeRead(
        Action<DateTime>? callback = null)
        => _dateTimeReadCallbacks.RegisterCallback(callback);

    /// <inheritdoc cref="TimeSystemMock.ICallbackHandler.TaskDelay(Action{TimeSpan})" />
    public Notification.IAwaitableCallback<TimeSpan> TaskDelay(
        Action<TimeSpan>? callback = null)
        => _taskDelayCallbacks.RegisterCallback(callback);

    /// <inheritdoc cref="TimeSystemMock.ICallbackHandler.ThreadSleep(Action{TimeSpan})" />
    public Notification.IAwaitableCallback<TimeSpan> ThreadSleep(
        Action<TimeSpan>? callback = null)
        => _threadSleepCallbacks.RegisterCallback(callback);

    #endregion

    public void InvokeDateTimeReadCallbacks(DateTime now)
        => _dateTimeReadCallbacks.InvokeCallbacks(now);

    public void InvokeTaskDelayCallbacks(TimeSpan delay)
        => _taskDelayCallbacks.InvokeCallbacks(delay);

    public void InvokeThreadSleepCallbacks(TimeSpan timeout)
        => _threadSleepCallbacks.InvokeCallbacks(timeout);
}