using System;
using System.Threading;
using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Testing.TimeSystem;

/// <summary>
///     The callback handler for the <see cref="MockTimeSystem" />.
/// </summary>
public interface INotificationHandler
{
#if FEATURE_PERIODIC_TIMER
	/// <summary>
	///     Notifications for the <see cref="IPeriodicTimer" />.
	/// </summary>
	IPeriodicTimerNotificationHandler PeriodicTimer { get; }
#endif

	/// <summary>
	///     Callback executed when any of the following <c>DateTime</c> read methods is called:<br />
	///     - <see cref="IDateTime.Now" /><br />
	///     - <see cref="IDateTime.UtcNow" /><br />
	///     - <see cref="IDateTime.Today" />
	/// </summary>
	/// <param name="callback">
	///     (optional) The callback to execute after <c>DateTime</c> was read.
	/// </param>
	/// <param name="predicate">
	///     (optional) A predicate used to filter which callbacks should be notified.<br />
	///     If set to <see langword="null" /> (default value) all callbacks are notified.
	/// </param>
	/// <returns>A <see cref="IAwaitableCallback{DateTime}" /> to un-register the callback on dispose.</returns>
	IAwaitableCallback<DateTime> DateTimeRead(
		Action<DateTime>? callback = null,
		Func<DateTime, bool>? predicate = null);

	/// <summary>
	///     Callback executed when any of the following <c>Task.Delay</c> overloads is called:<br />
	///     - <see cref="ITask.Delay(TimeSpan)" /><br />
	///     - <see cref="ITask.Delay(TimeSpan, CancellationToken)" /><br />
	///     - <see cref="ITask.Delay(int)" /><br />
	///     - <see cref="ITask.Delay(int, CancellationToken)" />
	/// </summary>
	/// <param name="callback">
	///     (optional) The callback to execute after the <c>Task.Delay</c> was called.
	/// </param>
	/// <param name="predicate">
	///     (optional) A predicate used to filter which callbacks should be notified.<br />
	///     If set to <see langword="null" /> (default value) all callbacks are notified.
	/// </param>
	/// <returns>A <see cref="IAwaitableCallback{TimeSpan}" /> to un-register the callback on dispose.</returns>
	IAwaitableCallback<TimeSpan> TaskDelay(
		Action<TimeSpan>? callback = null,
		Func<TimeSpan, bool>? predicate = null);

	/// <summary>
	///     Callback executed when any of the following <c>Thread.Sleep</c> overloads is called:<br />
	///     - <see cref="IThread.Sleep(TimeSpan)" /><br />
	///     - <see cref="IThread.Sleep(int)" />
	/// </summary>
	/// <param name="callback">
	///     (optional) The callback to execute after the <c>Thread.Sleep</c> was called.
	/// </param>
	/// <param name="predicate">
	///     (optional) A predicate used to filter which callbacks should be notified.<br />
	///     If set to <see langword="null" /> (default value) all callbacks are notified.
	/// </param>
	/// <returns>A <see cref="IAwaitableCallback{TimeSpan}" /> to un-register the callback on dispose.</returns>
	IAwaitableCallback<TimeSpan> ThreadSleep(
		Action<TimeSpan>? callback = null,
		Func<TimeSpan, bool>? predicate = null);

	/// <summary>
	///     Callback executed when the mocked time changed.
	/// </summary>
	/// <param name="callback">
	///     (optional) The callback to execute after the mocked time changed.
	/// </param>
	/// <param name="predicate">
	///     (optional) A predicate used to filter which changes should be notified.<br />
	///     The first parameter is the start time, the second parameter is the current time of the time provider.<br />
	///     If set to <see langword="null" /> (default value) all changes are notified.
	/// </param>
	/// <returns>A <see cref="IAwaitableCallback{TimeSpan}" /> to un-register the callback on dispose.</returns>
	IAwaitableCallback<DateTime> TimeChanged(
		Action<DateTime>? callback = null,
		Func<DateTime, DateTime, bool>? predicate = null);
}
