using System;

namespace Testably.Abstractions.TimeSystem;

internal sealed class DateTimeWrapper : IDateTime
{
	internal DateTimeWrapper(RealTimeSystem timeSystem)
	{
		TimeSystem = timeSystem;
	}

	#region IDateTime Members

	/// <inheritdoc cref="IDateTime.MaxValue" />
	public DateTime MaxValue
		=> DateTime.MaxValue;

	/// <inheritdoc cref="IDateTime.MinValue" />
	public DateTime MinValue
		=> DateTime.MinValue;

#if NETSTANDARD2_0
	/// <inheritdoc cref="IDateTime.UnixEpoch" />
	public DateTime UnixEpoch => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
#else
	/// <inheritdoc cref="IDateTime.UnixEpoch" />
	public DateTime UnixEpoch
		=> DateTime.UnixEpoch;
#endif

	/// <inheritdoc cref="IDateTime.Now" />
	public DateTime Now
		=> DateTime.Now;

	/// <inheritdoc cref="IDateTime.UtcNow" />
	public DateTime UtcNow
		=> DateTime.UtcNow;

	/// <inheritdoc cref="IDateTime.Today" />
	public DateTime Today
		=> DateTime.Today;

	/// <inheritdoc cref="ITimeSystemExtensionPoint.TimeSystem" />
	public ITimeSystem TimeSystem { get; }

	#endregion
}
