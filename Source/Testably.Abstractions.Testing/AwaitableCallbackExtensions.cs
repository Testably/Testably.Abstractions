using System;
using System.Threading;
using System.Threading.Tasks;
#if NET6_0_OR_GREATER
using System.Collections.Generic;
using System.Runtime.CompilerServices;
#endif

namespace Testably.Abstractions.Testing;

/// <summary>
///     Extension methods on <see cref="IAwaitableCallback{TValue}" />.
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

#if NET6_0_OR_GREATER
	/// <summary>
	///     Converts the <see cref="IAwaitableCallback{TValue}" /> to an <see cref="IAsyncEnumerable{TValue}" /> that yields a
	///     value each time the callback is executed.
	/// </summary>
	/// <remarks>
	///     Uses a default timeout of 30 seconds to prevent infinite waiting if the callback is never executed.
	/// </remarks>
	public static IAsyncEnumerable<TValue> ToAsyncEnumerable<TValue>(
		this IAwaitableCallback<TValue> source,
		CancellationToken cancellationToken = default)
		=> ToAsyncEnumerable(source, null, cancellationToken);

	/// <summary>
	///     Converts the <see cref="IAwaitableCallback{TValue}" /> to an <see cref="IAsyncEnumerable{TValue}" /> that yields a
	///     value each time the callback is executed.
	/// </summary>
	/// <remarks>
	///     If no <paramref name="timeout" /> is specified (<see langword="null" />), a default timeout of 30 seconds is used
	///     to prevent infinite waiting if the callback is never executed.
	/// </remarks>
	public static async IAsyncEnumerable<TValue> ToAsyncEnumerable<TValue>(
		this IAwaitableCallback<TValue> source,
		TimeSpan? timeout,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		using CancellationTokenSource cts =
			CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		cts.CancelAfter(timeout ?? TimeSpan.FromSeconds(30));
		CancellationToken token = cts.Token;

		while (!token.IsCancellationRequested)
		{
			TValue item;
			try
			{
				TValue[] items = await source.WaitAsync(cancellationToken: token);
				if (items.Length == 0)
				{
					continue;
				}

				item = items[0];
			}
			catch (OperationCanceledException)
			{
				yield break;
			}

			yield return item;
		}
	}

	/// <summary>
	///     Converts the <see cref="IAwaitableCallback{TValue}" /> to an <see cref="IAsyncEnumerable{TValue}" /> that yields a
	///     value each time the callback is executed.
	/// </summary>
	public static IAsyncEnumerable<TValue> ToAsyncEnumerable<TValue>(
		this IAwaitableCallback<TValue> source,
		int timeout,
		CancellationToken cancellationToken = default)
		=> ToAsyncEnumerable(source, TimeSpan.FromMilliseconds(timeout), cancellationToken);
#endif
}
