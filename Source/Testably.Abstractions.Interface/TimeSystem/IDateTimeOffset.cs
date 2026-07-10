using System;

namespace Testably.Abstractions.TimeSystem;

/// <summary>
///     Abstractions for <see cref="DateTimeOffset" />.
/// </summary>
public interface IDateTimeOffset : ITimeSystemEntity
{
	/// <inheritdoc cref="DateTimeOffset.MaxValue" />
	DateTimeOffset MaxValue { get; }

	/// <inheritdoc cref="DateTimeOffset.MinValue" />
	DateTimeOffset MinValue { get; }

	#pragma warning disable MA0202 // Branches differ only in XML doc comments
#if NETSTANDARD2_0
	/// <summary>
	///     The value of this constant is equivalent to 00:00:00.0000000 UTC, January 1, 1970, in the Gregorian calendar.
	///     <see cref="UnixEpoch" /> defines the point in time when Unix time is equal to 0.
	/// </summary>
	DateTimeOffset UnixEpoch { get; }
#else
	/// <inheritdoc cref="DateTimeOffset.UnixEpoch" />
	DateTimeOffset UnixEpoch { get; }
#endif
	#pragma warning restore MA0202

	/// <inheritdoc cref="DateTimeOffset.Now" />
	DateTimeOffset Now { get; }

	/// <inheritdoc cref="DateTimeOffset.UtcNow" />
	DateTimeOffset UtcNow { get; }
}
