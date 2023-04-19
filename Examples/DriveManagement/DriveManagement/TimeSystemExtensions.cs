using System;
using System.Threading;
using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Examples.DriveManagement;

/// <summary>
///     Extension methods for the time system.
/// </summary>
public static class TimeSystemExtensions
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
	public static DriveManagement CreateDriveManagement(
		this ITimeSystem timeSystem,
		TimeSpan interval,
		Action<CancellationToken> callback,
		Action<Exception>? onError = null)
	{
		return new DriveManagement(timeSystem, interval, callback, onError);
	}

	/// <summary>
	///     Tries to delay executing by the specified <paramref name="delay" />.
	///     <para />
	///     Returns <see langword="true" /> if the task delayed for the complete <paramref name="delay" />,
	///     <see langword="false" /> if the delay was aborted via the <paramref name="cancellationToken" />.
	/// </summary>
	public static bool TryDelay(
		this ITask task,
		TimeSpan delay,
		CancellationToken cancellationToken = default)
	{
		try
		{
			task.Delay(delay, cancellationToken).Wait(cancellationToken);
			return true;
		}
		catch (Exception)
		{
			// Ignore all exceptions:
			// https://stackoverflow.com/a/39885850
		}

		return false;
	}
}
