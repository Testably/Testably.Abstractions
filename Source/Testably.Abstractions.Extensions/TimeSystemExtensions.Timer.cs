using System;
using System.Threading;

namespace Testably.Abstractions;

public static partial class TimeSystemExtensions
{
	/// <summary>
	///     Create a timer that executes the <paramref name="callback" /> in a regular <paramref name="interval" />.
	/// </summary>
	/// <param name="timeSystem">The time system.</param>
	/// <param name="interval">The interval in which to execute the <paramref name="callback" />.</param>
	/// <param name="callback">The action to execute in each iteration.</param>
	/// <param name="onError">
	///     (optional) a callback for handling errors thrown by the <paramref name="callback" />.
	/// </param>
	public static IStoppedTimer CreateTimer(
		this ITimeSystem timeSystem,
		TimeSpan interval,
		Action<CancellationToken> callback,
		Action<Exception>? onError = null)
	{
		return new Timer.Timer(timeSystem, interval, callback, onError);
	}
}