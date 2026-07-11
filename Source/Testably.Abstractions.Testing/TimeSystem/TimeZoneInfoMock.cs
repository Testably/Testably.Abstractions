using System;
using System.Collections.ObjectModel;
using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Testing.TimeSystem;

internal sealed class TimeZoneInfoMock : ITimeZoneInfo
{
	private readonly MockTimeSystem _mockTimeSystem;

	internal TimeZoneInfoMock(MockTimeSystem timeSystem)
	{
		_mockTimeSystem = timeSystem;
	}

	#region ITimeZoneInfo Members

	/// <inheritdoc cref="ITimeZoneInfo.Local" />
	public TimeZoneInfo Local
		=> _mockTimeSystem.TimeProvider.LocalTimeZone;

	/// <inheritdoc cref="ITimeSystemEntity.TimeSystem" />
	public ITimeSystem TimeSystem
		=> _mockTimeSystem;

	/// <inheritdoc cref="ITimeZoneInfo.Utc" />
	public TimeZoneInfo Utc
		=> TimeZoneInfo.Utc;

	/// <inheritdoc cref="ITimeZoneInfo.FindSystemTimeZoneById(string)" />
	public TimeZoneInfo FindSystemTimeZoneById(string id)
		=> _mockTimeSystem.TimeProvider.FindSystemTimeZoneById(id);

	/// <inheritdoc cref="ITimeZoneInfo.GetSystemTimeZones()" />
	public ReadOnlyCollection<TimeZoneInfo> GetSystemTimeZones()
		=> _mockTimeSystem.TimeProvider.GetSystemTimeZones();

	#endregion
}
