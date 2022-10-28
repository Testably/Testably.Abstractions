using System;
using Testably.Abstractions.Testing.Internal;

namespace Testably.Abstractions.Testing;

/// <summary>
///     <see cref="MockTimeSystem.ITimeProvider" />s for use in the constructor of <see cref="MockTimeSystem" />.
/// </summary>
public static class TimeProvider
{
	/// <summary>
	///     Initializes the <see cref="MockTimeSystem.TimeProvider" /> with the current time.
	/// </summary>
	public static MockTimeSystem.ITimeProvider Now()
	{
		return new MockTimeSystem.TimeProviderMock(DateTime.UtcNow);
	}

	/// <summary>
	///     Initializes the <see cref="MockTimeSystem.TimeProvider" /> with a random time.
	///     <para />
	///     The random time increments the unix epoch by a random integer of seconds.
	/// </summary>
	public static MockTimeSystem.ITimeProvider Random()
	{
		DateTime randomTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
		   .AddSeconds(RandomFactory.Shared.Next());
		return new MockTimeSystem.TimeProviderMock(randomTime);
	}

	/// <summary>
	///     Initializes the <see cref="MockTimeSystem.TimeProvider" /> with the specified <paramref name="time" />.
	/// </summary>
	public static MockTimeSystem.ITimeProvider Use(DateTime time)
	{
		return new MockTimeSystem.TimeProviderMock(time);
	}
}