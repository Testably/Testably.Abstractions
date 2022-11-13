namespace Testably.Abstractions.Tests.TestHelpers;

public static class TimeTestHelper
{
	/// <summary>
	///     Applies a tolerance due to imprecise system clock.
	///     <para />
	///     The provided <paramref name="time" /> is reduced by 25ms.
	/// </summary>
	/// <param name="time">The time on which the tolerance should be applied.</param>
	public static DateTime ApplySystemClockTolerance(this DateTime time)
	{
		return time.AddMilliseconds(-25);
	}

	public static DateTime GetRandomTime(DateTimeKind kind = DateTimeKind.Unspecified)
	{
		Random random = new();
		return new DateTime(1970, 1, 1, 0, 0, 0, kind)
			.AddSeconds(random.Next());
	}
}