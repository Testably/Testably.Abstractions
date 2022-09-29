﻿using System;

namespace Testably.Abstractions.Testing;

/// <summary>
///     <see cref="TimeSystemMock.ITimeProvider" />s for use in the constructor of <see cref="TimeSystemMock" />.
/// </summary>
public static class TimeProvider
{
    /// <summary>
    ///     Initializes the <see cref="TimeSystemMock.TimeProvider" /> with the current time.
    /// </summary>
    public static TimeSystemMock.ITimeProvider Now()
    {
        return new TimeSystemMock.TimeProviderMock(DateTime.UtcNow);
    }

    /// <summary>
    ///     Initializes the <see cref="TimeSystemMock.TimeProvider" /> with a random time.
    ///     <para />
    ///     The random time increments the unix epoch by a random integer of seconds.
    /// </summary>
    public static TimeSystemMock.ITimeProvider Random()
    {
        DateTime randomTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
           .AddSeconds(new RandomSystem().Random.Shared.Next());
        return new TimeSystemMock.TimeProviderMock(randomTime);
    }

    /// <summary>
    ///     Initializes the <see cref="TimeSystemMock.TimeProvider" /> with the specified <paramref name="time" />.
    /// </summary>
    public static TimeSystemMock.ITimeProvider Use(DateTime time)
    {
        return new TimeSystemMock.TimeProviderMock(time);
    }
}