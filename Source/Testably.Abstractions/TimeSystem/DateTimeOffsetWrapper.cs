using System;

namespace Testably.Abstractions.TimeSystem;

internal sealed class DateTimeOffsetWrapper : IDateTimeOffset
{
	internal DateTimeOffsetWrapper(RealTimeSystem timeSystem)
	{
		TimeSystem = timeSystem;
	}

	#region IDateTimeOffset Members

	/// <inheritdoc cref="IDateTimeOffset.MaxValue" />
	public DateTimeOffset MaxValue
		=> DateTimeOffset.MaxValue;

	/// <inheritdoc cref="IDateTimeOffset.MinValue" />
	public DateTimeOffset MinValue
		=> DateTimeOffset.MinValue;

#if NETSTANDARD2_0
	/// <inheritdoc cref="IDateTimeOffset.UnixEpoch" />
	public DateTimeOffset UnixEpoch
		=> new(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
#else
	/// <inheritdoc cref="IDateTimeOffset.UnixEpoch" />
	public DateTimeOffset UnixEpoch
		=> DateTimeOffset.UnixEpoch;
#endif

	/// <inheritdoc cref="IDateTimeOffset.Now" />
	public DateTimeOffset Now
		=> DateTimeOffset.Now;

	/// <inheritdoc cref="IDateTimeOffset.UtcNow" />
	public DateTimeOffset UtcNow
		=> DateTimeOffset.UtcNow;

	/// <inheritdoc cref="ITimeSystemEntity.TimeSystem" />
	public ITimeSystem TimeSystem { get; }

	#endregion
}
