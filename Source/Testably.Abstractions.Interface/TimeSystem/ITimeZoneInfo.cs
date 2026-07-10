using System;
using System.Collections.ObjectModel;

namespace Testably.Abstractions.TimeSystem;

/// <summary>
///     Abstractions for <see cref="System.TimeZoneInfo" />.
/// </summary>
public interface ITimeZoneInfo : ITimeSystemEntity
{
	/// <inheritdoc cref="TimeZoneInfo.Local" />
	TimeZoneInfo Local { get; }

	/// <inheritdoc cref="TimeZoneInfo.Utc" />
	TimeZoneInfo Utc { get; }

	/// <inheritdoc cref="TimeZoneInfo.FindSystemTimeZoneById(string)" />
	TimeZoneInfo FindSystemTimeZoneById(string id);

	/// <inheritdoc cref="TimeZoneInfo.GetSystemTimeZones()" />
	ReadOnlyCollection<TimeZoneInfo> GetSystemTimeZones();
}
