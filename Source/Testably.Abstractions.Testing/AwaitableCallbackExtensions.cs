using System;
using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.Testing;

/// <summary>
///     Extension methods on <see cref="IAwaitableCallback{TValue}"/>.
/// </summary>
public static class AwaitableCallbackExtensions
{
	/// <summary>
	///     Blocks the current thread until the callback is executed.
	///     <para />
	///     Throws a <see cref="TimeoutException" /> if the <paramref name="timeout" /> expired before the callback was
	///     executed.
	/// </summary>
	/// <param name="callback">The callback.</param>
	/// <param name="count">
	///     (optional) The number of callbacks to wait.<br />
	///     Defaults to <c>1</c>
	/// </param>
	/// <param name="timeout">
	///     (optional) The timeout in milliseconds to wait on the callback.<br />
	///     Defaults to <c>30000</c>ms (30 seconds).
	/// </param>
	public static TValue[] Wait<TValue>(this IAwaitableCallback<TValue> callback,
		int count = 1,
		int timeout = 30000)
		=> callback.Wait(count, TimeSpan.FromMilliseconds(timeout));

	/// <summary>
	///     Waits asynchronously until the callback is executed.
	/// </summary>
	/// <param name="callback">The callback.</param>
	/// <param name="count">
	///     (optional) The number of callbacks to wait.<br />
	///     Defaults to <c>1</c>
	/// </param>
	/// <param name="timeout">
	///     (optional) The timeout in milliseconds to wait on the callback.<br />
	///     Defaults to <c>30000</c>ms (30 seconds).
	/// </param>
	/// <param name="cancellationToken">
	///     (optional) A <see cref="CancellationToken" /> to cancel waiting.<br />
	///     Throws a <see cref="OperationCanceledException" /> if the token was canceled before the callback was executed.
	/// </param>
	public static Task<TValue[]> WaitAsync<TValue>(this IAwaitableCallback<TValue> callback,
		int count = 1,
		int timeout = 30000,
		CancellationToken? cancellationToken = null)
		=> callback.WaitAsync(count, TimeSpan.FromMilliseconds(timeout), cancellationToken);
}
