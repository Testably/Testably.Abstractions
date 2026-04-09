#if FEATURE_PERIODIC_TIMER
using System;
using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Testing.TimeSystem;

/// <summary>
///     The callback handler for the <see cref="IPeriodicTimer" /> of the <see cref="MockTimeSystem" />.
/// </summary>
public interface IPeriodicTimerNotificationHandler
{
	/// <summary>
	///     Callback executed when any periodic timer is waiting for the next tick.
	/// </summary>
	/// <param name="callback">
	///     (optional) The callback to execute when the periodic timer is waiting for the next tick. The parameter is the periodic timer which is waiting for the next tick.
	/// </param>
	/// <param name="predicate">
	///     (optional) A predicate used to filter which callbacks should be notified.<br />
	///     If set to <see langword="null" /> (default value) all callbacks are notified.
	/// </param>
	/// <returns>A <see cref="IAwaitableCallback{IPeriodicTimer}" /> to un-register the callback on dispose.</returns>
	IAwaitableCallback<IPeriodicTimer> WaitingForNextTick(
		Action<IPeriodicTimer>? callback = null,
		Func<IPeriodicTimer, bool>? predicate = null);
}
#endif
