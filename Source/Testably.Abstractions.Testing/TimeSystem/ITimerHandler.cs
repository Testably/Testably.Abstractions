using System;
using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.Testing.TimeSystem;

/// <summary>
///     The timer handler gives access to all registered timers.
/// </summary>
public interface ITimerHandler
{
	/// <summary>
	///     Gets the timer associated with the specified <paramref name="index" />.
	/// </summary>
	/// <param name="index">A zero-based incremented integer according to the order of the timer registration.</param>
	ITimerMock this[int index] { get; }

	/// <summary>
	///     Moves the clock to the earliest scheduled timer, also a timeout that would not move the clock on its own,
	///     fires it and blocks the current thread until its callback completes or waits for the clock.
	/// </summary>
	/// <remarks>
	///     With auto-advance, the timers that move the clock on their own may fire before, so that the earliest
	///     remaining timer fires. The timers that the callback schedules are not waited for; see
	///     <see cref="WaitUntilIdle(int)" />.
	/// </remarks>
	/// <param name="timeout">
	///     The timeout in milliseconds for the callback.<br />
	///     Throws a <see cref="TimeoutException" /> when the timeout is expired.
	/// </param>
	/// <returns><see langword="false" />, when no timer is scheduled, otherwise <see langword="true" />.</returns>
	/// <exception cref="TimeoutException">
	///     When the callback does not complete within the <paramref name="timeout" />, or when the scheduler failed with a
	///     strict timer order. With a strict timer order, an expired <paramref name="timeout" /> fails the scheduler, so
	///     that every later move of the clock throws as well.
	/// </exception>
	bool AdvanceToNextTimer(int timeout = 10000);

	/// <summary>
	///     Blocks the current thread until the timers are idle: no timer is due or advances the clock on its own, and no
	///     timer callback runs, except for callbacks that wait for the clock.
	/// </summary>
	/// <remarks>
	///     The timers include the timers of the <c>TimeProvider</c> from <c>ToTimeProvider()</c>, but not the waits that
	///     move or wait for the clock on the calling thread, like <c>Task.Delay</c>, <c>Thread.Sleep</c> or the
	///     <c>PeriodicTimer</c> of the <see cref="MockTimeSystem" />. A periodic <c>ITimer</c> with auto-advance never
	///     becomes idle. A callback that
	///     waited for the clock is no longer tracked once it continues, and neither is the work that the callbacks start
	///     elsewhere, e.g. the continuations of a completed delay on the thread pool. Inside a timer callback, the timers
	///     cannot become idle, as the callback itself still runs.
	/// </remarks>
	/// <param name="timeout">
	///     The timeout in milliseconds.<br />
	///     Throws a <see cref="TimeoutException" /> when the timeout is expired.
	/// </param>
	/// <exception cref="TimeoutException">
	///     When the timers do not become idle within the <paramref name="timeout" />. The message describes what keeps
	///     them busy. Also when the scheduler failed with a strict timer order.
	/// </exception>
	void WaitUntilIdle(int timeout = 10000);

	/// <summary>
	///     Completes when the timers are idle: no timer is due or advances the clock on its own, and no timer callback
	///     runs, except for callbacks that wait for the clock.
	/// </summary>
	/// <remarks>
	///     The timers include the timers of the <c>TimeProvider</c> from <c>ToTimeProvider()</c>, but not the waits that
	///     move or wait for the clock on the calling thread, like <c>Task.Delay</c>, <c>Thread.Sleep</c> or the
	///     <c>PeriodicTimer</c> of the <see cref="MockTimeSystem" />. A periodic <c>ITimer</c> with auto-advance never
	///     becomes idle. A callback that
	///     waited for the clock is no longer tracked once it continues, and neither is the work that the callbacks start
	///     elsewhere, e.g. the continuations of a completed delay on the thread pool. Inside a timer callback, the timers
	///     cannot become idle, as the callback itself still runs.
	/// </remarks>
	/// <param name="timeout">
	///     The timeout in milliseconds.<br />
	///     Throws a <see cref="TimeoutException" /> when the timeout is expired.
	/// </param>
	/// <param name="cancellationToken">A token to cancel the wait.</param>
	/// <exception cref="TimeoutException">
	///     When the timers do not become idle within the <paramref name="timeout" />. The message describes what keeps
	///     them busy. Also when the scheduler failed with a strict timer order.
	/// </exception>
	Task WaitUntilIdleAsync(int timeout = 10000, CancellationToken cancellationToken = default);
}
