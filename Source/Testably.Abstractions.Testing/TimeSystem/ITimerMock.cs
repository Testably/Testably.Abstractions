using System;
using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Testing.TimeSystem;

/// <summary>
///     Additional abstractions for a mocked timer to simplify testing.<br />
///     Implements <see cref="ITimer" />.
/// </summary>
public interface ITimerMock : ITimer
{
	/// <summary>
	///     Blocks the current thread, until the timer is executed <paramref name="executionCount" /> times.<br />
	///     Throws an <see cref="TimeoutException" />
	/// </summary>
	/// <param name="executionCount">The number of execution cycles the thread is blocked.</param>
	/// <param name="timeout">
	///     The timeout for blocking the current thread.<br />
	///     Throws an <see cref="TimeoutException" /> when the timeout is expired.
	/// </param>
	/// <param name="callback">
	///     A callback to execute after the execution count, before the next execution is triggered.
	///     <para />
	///     The method parameter allows stopping the timer by calling <see cref="IDisposable.Dispose()" />.
	/// </param>
	/// <returns></returns>
	/// <exception cref="TimeoutException">
	///     When the <paramref name="timeout" /> expires before the timer is executed
	///     <paramref name="executionCount" /> times.
	/// </exception>
	ITimerMock Wait(int executionCount = 1, int timeout = 10000,
		Action<ITimerMock>? callback = null);
}
