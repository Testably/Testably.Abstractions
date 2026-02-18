using System;
using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.Testing;

/// <summary>
///     An object to allow<br />
///     - un-registering a callback by calling <see cref="IDisposable.Dispose()" /><br />
///     - blocking for the callback to be executed
/// </summary>
public interface IAwaitableCallback<TValue> : IDisposable
{
	/// <summary>
	///     Blocks the current thread until the callback is executed.
	///     <para />
	///     Throws a <see cref="TimeoutException" /> if the <paramref name="timeout" /> expired before the callback was
	///     executed.
	/// </summary>
	/// <param name="filter">
	///     (optional) A filter for the callback to count.<br />
	///     Defaults to <see langword="null" /> which will consider all callbacks.
	/// </param>
	/// <param name="timeout">
	///     (optional) The timeout in milliseconds to wait on the callback.<br />
	///     Defaults to <c>30000</c>ms (30 seconds).
	/// </param>
	/// <param name="count">
	///     (optional) The number of callbacks to wait.<br />
	///     Defaults to <c>1</c>
	/// </param>
	/// <param name="executeWhenWaiting">
	///     (optional) A callback to execute when waiting started.
	/// </param>
#if MarkExecuteWhileWaitingNotificationObsolete
	[Obsolete("Use another `Wait` or `WaitAsync` overload and move the filter to the creation of the awaitable callback.")]
#endif
	void Wait(Func<TValue, bool>? filter,
		int timeout = 30000,
		int count = 1,
		Action? executeWhenWaiting = null);
	
	/// <summary>
	///     Blocks the current thread until the callback is executed.
	///     <para />
	///     Throws a <see cref="TimeoutException" /> if the <paramref name="timeout" /> expired before the callback was
	///     executed.
	/// </summary>
	/// <param name="count">
	///     (optional) The number of callbacks to wait.<br />
	///     Defaults to <c>1</c>
	/// </param>
	/// <param name="timeout">
	///     (optional) The timeout to wait on the callback.<br />
	///     If not specified (<see langword="null" />), defaults to 30 seconds.
	/// </param>
	TValue[] Wait(int count = 1, TimeSpan? timeout = null);

	/// <summary>
	///     Waits asynchronously until the callback is executed.
	/// </summary>
	/// <param name="count">
	///     (optional) The number of callbacks to wait.<br />
	///     Defaults to <c>1</c>
	/// </param>
	/// <param name="timeout">
	///     (optional) The timeout to wait on the callback.<br />
	///     If not specified (<see langword="null" />), defaults to 30 seconds.
	/// </param>
	/// <param name="cancellationToken">
	///     (optional) A <see cref="CancellationToken" /> to cancel waiting.<br />
	///     Throws a <see cref="OperationCanceledException" /> if the token was canceled before the callback was executed.
	/// </param>
	Task<TValue[]> WaitAsync(
		int count = 1,
		TimeSpan? timeout = null,
		CancellationToken? cancellationToken = null);
}
