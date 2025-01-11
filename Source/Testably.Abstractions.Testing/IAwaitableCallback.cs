using System;

namespace Testably.Abstractions.Testing;

/// <summary>
///     An object to allow<br />
///     - un-registering a callback by calling <see cref="IDisposable.Dispose()" /><br />
///     - blocking for the callback to be executed
/// </summary>
public interface IAwaitableCallback<out TValue> : IDisposable
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
	void Wait(Func<TValue, bool>? filter = null,
		int timeout = 30000,
		int count = 1,
		Action? executeWhenWaiting = null);
}
