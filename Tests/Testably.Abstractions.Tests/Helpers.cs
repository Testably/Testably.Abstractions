using System;

namespace Testably.Abstractions.Tests;

public static class Helpers
{
    /// <summary>
    ///     Applies a tolerance due to imprecise system clock.
    ///     <para />
    ///     The provided <paramref name="time" /> is reduced by 15ms.
    /// </summary>
    /// <param name="time">The time on which the tolerance should be applied.</param>
    public static DateTime ApplySystemClockTolerance(this DateTime time)
    {
        return time.AddMilliseconds(-15);
    }
}