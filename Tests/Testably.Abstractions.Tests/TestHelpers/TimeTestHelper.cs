using FluentAssertions.Primitives;

namespace Testably.Abstractions.Tests.TestHelpers;

public static class TimeTestHelper
{
	/// <summary>
	///     Asserts that the current <see cref="DateTime" /> is between the <paramref name="startTime" /> and
	///     <paramref name="endTime" /> while applying the system clock tolerance.
	/// </summary>
	public static AndConstraint<DateTimeAssertions> BeBetween(this DateTimeAssertions assertion,
		DateTime startTime, DateTime endTime,
		TimeSpan? tolerance = null,
		string because = "", params object[] becauseArgs)
	{
		tolerance ??= TimeSpan.FromMilliseconds(35);
		return assertion
			.BeOnOrAfter(startTime - tolerance.Value, because, becauseArgs).And
			.BeOnOrBefore(endTime + tolerance.Value, because, becauseArgs);
	}

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
