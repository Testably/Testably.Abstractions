namespace Testably.Abstractions.Tests.TestHelpers;

public static class TimeComparison
{
	/// <summary>
	///     The system clock tolerance is 35 milliseconds.
	/// </summary>
	public static readonly TimeSpan Tolerance = TimeSpan.FromMilliseconds(35);

	/// <summary>
	///     Applies a tolerance due to imprecise system clock.
	///     <para />
	///     The provided <paramref name="time" /> is reduced by <paramref name="tolerance" />.
	/// </summary>
	/// <param name="time">The time on which the tolerance should be applied.</param>
	/// <param name="tolerance">
	///     The tolerance in milliseconds to apply on the <paramref name="time" />.<br />
	///     Defaults to 35ms.
	/// </param>
	public static DateTime ApplySystemClockTolerance(this DateTime time, int tolerance = 35)
	{
		return time.AddMilliseconds(-1 * tolerance);
	}
}
