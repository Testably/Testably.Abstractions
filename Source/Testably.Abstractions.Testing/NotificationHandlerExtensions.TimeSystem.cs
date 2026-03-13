using System;
using Testably.Abstractions.Testing.TimeSystem;

namespace Testably.Abstractions.Testing;

/// <summary>
///     Extension methods for the <see cref="INotificationHandler" />
/// </summary>
public static partial class NotificationHandlerExtensions
{
	/// <summary>
	///     Callback executed when the mocked time changed after the specified <paramref name="interval" />.
	/// </summary>
	/// <param name="handler">The notification handler.</param>
	/// <param name="interval">The time interval that should elapse in the mock time system.</param>
	/// <param name="callback">
	///     (optional) The callback to execute after the mocked time changed.
	/// </param>
	/// <returns>A <see cref="IAwaitableCallback{ChangeDescription}" /> to un-register the callback on dispose.</returns>
	public static IAwaitableCallback<DateTime> Elapsed(
		this INotificationHandler handler,
		TimeSpan interval,
		Action<DateTime>? callback = null)
		=> handler.TimeChanged(callback, (s, d) => d - s >= interval);

	/// <summary>
	///     Callback executed when the mocked time changed.
	/// </summary>
	/// <param name="handler">The notification handler.</param>
	/// <param name="callback">
	///     (optional) The callback to execute after the mocked time changed.
	/// </param>
	/// <param name="predicate">
	///     (optional) A predicate used to filter which changes should be notified.<br />
	///     If set to <see langword="null" /> (default value) all changes are notified.
	/// </param>
	/// <returns>A <see cref="IAwaitableCallback{TimeSpan}" /> to un-register the callback on dispose.</returns>
	public static IAwaitableCallback<DateTime> TimeChanged(
		this INotificationHandler handler,
		Action<DateTime>? callback = null,
		Func<DateTime, bool>? predicate = null)
		=> handler.TimeChanged(callback, predicate is null ? null : (_, d) => predicate(d));
}
