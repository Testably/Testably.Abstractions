using System;

namespace Testably.Abstractions.TimeSystem;

/// <summary>
///     Abstractions for <see cref="DateTime" />.
/// </summary>
public interface IDateTime : ITimeSystemExtensionPoint
{
	/// <inheritdoc cref="DateTime.MaxValue" />
	DateTime MaxValue { get; }

	/// <inheritdoc cref="DateTime.MinValue" />
	DateTime MinValue { get; }

#if NETSTANDARD2_0
	/// <summary>
	///     The value of this constant is equivalent to 00:00:00.0000000 UTC, January 1, 1970, in the Gregorian calendar.
	///     <see cref="UnixEpoch" /> defines the point in time when Unix time is equal to 0.
	/// </summary>
	DateTime UnixEpoch { get; }
#else
	/// <inheritdoc cref="DateTime.UnixEpoch" />
	DateTime UnixEpoch { get; }
#endif

	/// <inheritdoc cref="DateTime.Now" />
	DateTime Now { get; }

	/// <inheritdoc cref="DateTime.UtcNow" />
	DateTime UtcNow { get; }

	/// <inheritdoc cref="DateTime.Today" />
	DateTime Today { get; }
}
