#if !NET8_0_OR_GREATER
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace System;

public static class CancellationTokenSourceExtensions
{
	/// <summary>
	///     Communicates a request for cancellation asynchronously.
	/// </summary>
	public static Task CancelAsync(this CancellationTokenSource @this)
	{
		@this.Cancel();
		return Task.CompletedTask;
	}
}
#endif
