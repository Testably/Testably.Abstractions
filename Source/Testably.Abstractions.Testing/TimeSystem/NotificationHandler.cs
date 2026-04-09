using System;
#if FEATURE_PERIODIC_TIMER
using Testably.Abstractions.TimeSystem;
#endif

namespace Testably.Abstractions.Testing.TimeSystem;

internal sealed class NotificationHandler(MockTimeSystem mockTimeSystem)
#if FEATURE_PERIODIC_TIMER
	: INotificationHandler, IPeriodicTimerNotificationHandler
#else
	: INotificationHandler
#endif
{
	private readonly Notification.INotificationFactory<DateTime>
		_dateTimeReadCallbacks = Notification.CreateFactory<DateTime>();

#if FEATURE_PERIODIC_TIMER
	private readonly Notification.INotificationFactory<IPeriodicTimer>
		_periodicTimerWaitingForNextTickCallbacks = Notification.CreateFactory<IPeriodicTimer>();
#endif

	private readonly Notification.INotificationFactory<TimeSpan>
		_taskDelayCallbacks = Notification.CreateFactory<TimeSpan>();

	private readonly Notification.INotificationFactory<TimeSpan>
		_threadSleepCallbacks = Notification.CreateFactory<TimeSpan>();

	private readonly Notification.INotificationFactory<DateTime>
		_timeChangedCallbacks = Notification.CreateFactory<DateTime>();

	#region INotificationHandler Members

#if FEATURE_PERIODIC_TIMER
	/// <inheritdoc cref="INotificationHandler.PeriodicTimer" />
	public IPeriodicTimerNotificationHandler PeriodicTimer => this;
#endif

	/// <inheritdoc cref="INotificationHandler.DateTimeRead(Action{DateTime}?, Func{DateTime, bool}?)" />
	public IAwaitableCallback<DateTime> DateTimeRead(
		Action<DateTime>? callback = null,
		Func<DateTime, bool>? predicate = null)
		=> _dateTimeReadCallbacks.RegisterCallback(callback, predicate);

	/// <inheritdoc cref="INotificationHandler.TaskDelay(Action{TimeSpan}?, Func{TimeSpan, bool}?)" />
	public IAwaitableCallback<TimeSpan> TaskDelay(
		Action<TimeSpan>? callback = null,
		Func<TimeSpan, bool>? predicate = null)
		=> _taskDelayCallbacks.RegisterCallback(callback, predicate);

	/// <inheritdoc cref="INotificationHandler.ThreadSleep(Action{TimeSpan}?, Func{TimeSpan, bool}?)" />
	public IAwaitableCallback<TimeSpan> ThreadSleep(
		Action<TimeSpan>? callback = null,
		Func<TimeSpan, bool>? predicate = null)
		=> _threadSleepCallbacks.RegisterCallback(callback, predicate);

	/// <inheritdoc cref="INotificationHandler.TimeChanged(Action{DateTime}?, Func{DateTime, DateTime, bool}?)" />
	public IAwaitableCallback<DateTime> TimeChanged(
		Action<DateTime>? callback = null,
		Func<DateTime, DateTime, bool>? predicate = null)
		=> _timeChangedCallbacks.RegisterCallback(callback,
			predicate is null ? null : d => predicate(mockTimeSystem.TimeProvider.StartTime, d));

	#endregion

	#region IPeriodicTimerNotificationHandler Members

#if FEATURE_PERIODIC_TIMER

	#region IPeriodicTimerNotificationHandler Members

	/// <inheritdoc cref="IPeriodicTimerNotificationHandler.WaitingForNextTick(Action{IPeriodicTimer}?, Func{IPeriodicTimer, bool}?)" />
	public IAwaitableCallback<IPeriodicTimer> WaitingForNextTick(
		Action<IPeriodicTimer>? callback = null, Func<IPeriodicTimer, bool>? predicate = null)
		=> _periodicTimerWaitingForNextTickCallbacks.RegisterCallback(callback, predicate);

	#endregion

#endif

	#endregion

	public void InvokeDateTimeReadCallbacks(DateTime now)
		=> _dateTimeReadCallbacks.InvokeCallbacks(now);

#if FEATURE_PERIODIC_TIMER
	public void InvokePeriodicTimerWaitingForNextTick(IPeriodicTimer timer)
		=> _periodicTimerWaitingForNextTickCallbacks.InvokeCallbacks(timer);
#endif

	public void InvokeTaskDelayCallbacks(TimeSpan delay)
		=> _taskDelayCallbacks.InvokeCallbacks(delay);

	public void InvokeThreadSleepCallbacks(TimeSpan timeout)
		=> _threadSleepCallbacks.InvokeCallbacks(timeout);

	public void InvokeTimeChanged(DateTime now)
		=> _timeChangedCallbacks.InvokeCallbacks(now);
}
