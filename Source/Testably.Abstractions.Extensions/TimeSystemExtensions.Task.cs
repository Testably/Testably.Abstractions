using System;
using System.Threading;

namespace Testably.Abstractions;

/// <summary>
///     Extension methods for the time system.
/// </summary>
public static partial class TimeSystemExtensions
{
	/// <summary>
	///     Tries to delay executing by the specified <paramref name="delay" />.
	///     <para />
	///     Returns <see langword="true" /> if the Task delayed for the complete <paramref name="delay" />,
	///     <see langword="false" /> if the delay was aborted via the <paramref name="cancellationToken" />.
	/// </summary>
	public static bool TryDelay(
		this ITimeSystem.ITask timeSystem,
		TimeSpan delay,
		CancellationToken cancellationToken = default)
	{
		try
		{
			timeSystem.Delay(delay, cancellationToken).Wait(cancellationToken);
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