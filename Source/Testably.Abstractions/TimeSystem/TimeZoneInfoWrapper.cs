using System;
using System.Collections.ObjectModel;

namespace Testably.Abstractions.TimeSystem;

internal sealed class TimeZoneInfoWrapper : ITimeZoneInfo
{
	internal TimeZoneInfoWrapper(RealTimeSystem timeSystem)
	{
		TimeSystem = timeSystem;
	}

	#region ITimeZoneInfo Members

	/// <inheritdoc cref="ITimeZoneInfo.Local" />
	public TimeZoneInfo Local
		=> TimeZoneInfo.Local;

	/// <inheritdoc cref="ITimeSystemEntity.TimeSystem" />
	public ITimeSystem TimeSystem { get; }

	/// <inheritdoc cref="ITimeZoneInfo.Utc" />
	public TimeZoneInfo Utc
		=> TimeZoneInfo.Utc;

	/// <inheritdoc cref="ITimeZoneInfo.FindSystemTimeZoneById(string)" />
	public TimeZoneInfo FindSystemTimeZoneById(string id)
		=> TimeZoneInfo.FindSystemTimeZoneById(id);

	/// <inheritdoc cref="ITimeZoneInfo.GetSystemTimeZones()" />
	public ReadOnlyCollection<TimeZoneInfo> GetSystemTimeZones()
		=> TimeZoneInfo.GetSystemTimeZones();

	#endregion
}
