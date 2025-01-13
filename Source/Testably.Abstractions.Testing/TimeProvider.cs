using System;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.Testing.TimeSystem;

namespace Testably.Abstractions.Testing;

/// <summary>
///     <see cref="ITimeProvider" />s for use in the constructor of <see cref="MockTimeSystem" />.
/// </summary>
public static class TimeProvider
{
	/// <summary>
	///     Initializes the <see cref="MockTimeSystem.TimeProvider" /> with the current time.
	/// </summary>
	public static ITimeProvider Now()
	{
		return new TimeProviderMock(DateTime.UtcNow, "Now");
	}

	/// <summary>
	///     Initializes the <see cref="MockTimeSystem.TimeProvider" /> with a random time.
	///     <para />
	///     The random time increments the unix epoch by a random integer of seconds.
	/// </summary>
	public static ITimeProvider Random()
	{
		#pragma warning disable MA0113 // Use DateTime.UnixEpoch
		DateTime randomTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
			.AddSeconds(RandomFactory.Shared.Next());
		#pragma warning restore MA0113
		return new TimeProviderMock(randomTime, "Random");
	}

	/// <summary>
	///     Initializes the <see cref="MockTimeSystem.TimeProvider" /> with the specified <paramref name="time" />.
	/// </summary>
	public static ITimeProvider Use(DateTime time)
	{
		return new TimeProviderMock(time, "Fixed");
	}
}
