using System;

namespace Testably.Abstractions.Testing.TimeSystem;

internal sealed class NotificationHandler : INotificationHandler
{
	private readonly Notification.INotificationFactory<DateTime>
		_dateTimeReadCallbacks = Notification.CreateFactory<DateTime>();

	private readonly Notification.INotificationFactory<TimeSpan>
		_taskDelayCallbacks = Notification.CreateFactory<TimeSpan>();

	private readonly Notification.INotificationFactory<TimeSpan>
		_threadSleepCallbacks = Notification.CreateFactory<TimeSpan>();

	private readonly Notification.INotificationFactory<TimerExecution>
		_timerExecutedCallbacks = Notification.CreateFactory<TimerExecution>();

	#region INotificationHandler Members

	/// <inheritdoc cref="INotificationHandler.DateTimeRead(Action{DateTime}?, Func{DateTime, bool}?)" />
	public Notification.IAwaitableCallback<DateTime> DateTimeRead(
		Action<DateTime>? callback = null,
		Func<DateTime, bool>? predicate = null)
		=> _dateTimeReadCallbacks.RegisterCallback(callback, predicate);

	/// <inheritdoc cref="INotificationHandler.TaskDelay(Action{TimeSpan}?, Func{TimeSpan, bool}?)" />
	public Notification.IAwaitableCallback<TimeSpan> TaskDelay(
		Action<TimeSpan>? callback = null,
		Func<TimeSpan, bool>? predicate = null)
		=> _taskDelayCallbacks.RegisterCallback(callback, predicate);

	/// <inheritdoc cref="INotificationHandler.ThreadSleep(Action{TimeSpan}?, Func{TimeSpan, bool}?)" />
	public Notification.IAwaitableCallback<TimeSpan> ThreadSleep(
		Action<TimeSpan>? callback = null,
		Func<TimeSpan, bool>? predicate = null)
		=> _threadSleepCallbacks.RegisterCallback(callback, predicate);

	/// <inheritdoc cref="INotificationHandler.TimerExecuted(Action{TimerExecution}?, Func{TimerExecution, bool}?)" />
	public Notification.IAwaitableCallback<TimerExecution> TimerExecuted(
		Action<TimerExecution>? callback = null,
		Func<TimerExecution, bool>? predicate = null)
		=> _timerExecutedCallbacks.RegisterCallback(callback, predicate);

	#endregion

	public void InvokeDateTimeReadCallbacks(DateTime now)
		=> _dateTimeReadCallbacks.InvokeCallbacks(now);

	public void InvokeTaskDelayCallbacks(TimeSpan delay)
		=> _taskDelayCallbacks.InvokeCallbacks(delay);

	public void InvokeThreadSleepCallbacks(TimeSpan timeout)
		=> _threadSleepCallbacks.InvokeCallbacks(timeout);

	public void InvokeTimerExecutedCallbacks(TimerExecution timerExecution)
		=> _timerExecutedCallbacks.InvokeCallbacks(timerExecution);
}
