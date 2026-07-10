using System;
using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Testing.TimeSystem;

internal sealed class DateTimeOffsetMock : IDateTimeOffset
{
	private readonly NotificationHandler _callbackHandler;
	private readonly MockTimeSystem _mockTimeSystem;

	internal DateTimeOffsetMock(MockTimeSystem timeSystem,
		NotificationHandler callbackHandler)
	{
		_mockTimeSystem = timeSystem;
		_callbackHandler = callbackHandler;
	}

	#region IDateTimeOffset Members

	/// <inheritdoc cref="IDateTimeOffset.MaxValue" />
	public DateTimeOffset MaxValue
		=> new(_mockTimeSystem.TimeProvider.MaxValue.Ticks, TimeSpan.Zero);

	/// <inheritdoc cref="IDateTimeOffset.MinValue" />
	public DateTimeOffset MinValue
		=> new(_mockTimeSystem.TimeProvider.MinValue.Ticks, TimeSpan.Zero);

	/// <inheritdoc cref="IDateTimeOffset.Now" />
	public DateTimeOffset Now
	{
		get
		{
			DateTimeOffset value = new(_mockTimeSystem.TimeProvider.Read().ToLocalTime());
			_callbackHandler.InvokeDateTimeOffsetReadCallbacks(value);
			return value;
		}
	}

	/// <inheritdoc cref="ITimeSystemEntity.TimeSystem" />
	public ITimeSystem TimeSystem
		=> _mockTimeSystem;

	/// <inheritdoc cref="IDateTimeOffset.UnixEpoch" />
	public DateTimeOffset UnixEpoch
		=> new(_mockTimeSystem.TimeProvider.UnixEpoch.Ticks, TimeSpan.Zero);

	/// <inheritdoc cref="IDateTimeOffset.UtcNow" />
	public DateTimeOffset UtcNow
	{
		get
		{
			DateTimeOffset value = new(_mockTimeSystem.TimeProvider.Read().ToUniversalTime());
			_callbackHandler.InvokeDateTimeOffsetReadCallbacks(value);
			return value;
		}
	}

	#endregion
}
