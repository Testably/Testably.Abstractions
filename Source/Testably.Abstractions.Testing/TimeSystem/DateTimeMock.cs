using System;
using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Testing.TimeSystem;

internal sealed class DateTimeMock : IDateTime
{
	private readonly NotificationHandler _callbackHandler;
	private readonly MockTimeSystem _mockTimeSystem;

	internal DateTimeMock(MockTimeSystem timeSystem,
						  NotificationHandler callbackHandler)
	{
		_mockTimeSystem = timeSystem;
		_callbackHandler = callbackHandler;
	}

	#region IDateTime Members

	/// <inheritdoc cref="IDateTime.MaxValue" />
	public DateTime MaxValue
		=> _mockTimeSystem.TimeProvider.MaxValue;

	/// <inheritdoc cref="IDateTime.MinValue" />
	public DateTime MinValue
		=> _mockTimeSystem.TimeProvider.MinValue;

	/// <inheritdoc cref="IDateTime.Now" />
	public DateTime Now
	{
		get
		{
			DateTime value = _mockTimeSystem.TimeProvider.Read().ToLocalTime();
			_callbackHandler.InvokeDateTimeReadCallbacks(value);
			return value;
		}
	}

	/// <inheritdoc cref="ITimeSystemExtensionPoint.TimeSystem" />
	public ITimeSystem TimeSystem
		=> _mockTimeSystem;

	/// <inheritdoc cref="IDateTime.Today" />
	public DateTime Today
	{
		get
		{
			DateTime value = _mockTimeSystem.TimeProvider.Read().Date;
			_callbackHandler.InvokeDateTimeReadCallbacks(value);
			return value;
		}
	}

	/// <inheritdoc cref="IDateTime.UnixEpoch" />
	public DateTime UnixEpoch
		=> _mockTimeSystem.TimeProvider.UnixEpoch;

	/// <inheritdoc cref="IDateTime.UtcNow" />
	public DateTime UtcNow
	{
		get
		{
			DateTime value = _mockTimeSystem.TimeProvider.Read().ToUniversalTime();
			_callbackHandler.InvokeDateTimeReadCallbacks(value);
			return value;
		}
	}

	#endregion
}