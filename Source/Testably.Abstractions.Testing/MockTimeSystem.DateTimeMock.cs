using System;

namespace Testably.Abstractions.Testing;

public sealed partial class MockTimeSystem
{
	private sealed class DateTimeMock : IDateTime
	{
		private readonly TimeSystemMockCallbackHandler _callbackHandler;
		private readonly MockTimeSystem _timeSystemMock;

		internal DateTimeMock(MockTimeSystem timeSystem,
		                      TimeSystemMockCallbackHandler callbackHandler)
		{
			_timeSystemMock = timeSystem;
			_callbackHandler = callbackHandler;
		}

		#region IDateTime Members

		/// <inheritdoc cref="IDateTime.MaxValue" />
		public DateTime MaxValue
			=> _timeSystemMock.TimeProvider.MaxValue;

		/// <inheritdoc cref="IDateTime.MinValue" />
		public DateTime MinValue
			=> _timeSystemMock.TimeProvider.MinValue;

		/// <inheritdoc cref="IDateTime.Now" />
		public DateTime Now
		{
			get
			{
				DateTime value = _timeSystemMock.TimeProvider.Read().ToLocalTime();
				_callbackHandler.InvokeDateTimeReadCallbacks(value);
				return value;
			}
		}

		/// <inheritdoc cref="ITimeSystemExtensionPoint.TimeSystem" />
		public ITimeSystem TimeSystem
			=> _timeSystemMock;

		/// <inheritdoc cref="IDateTime.Today" />
		public DateTime Today
		{
			get
			{
				DateTime value = _timeSystemMock.TimeProvider.Read().Date;
				_callbackHandler.InvokeDateTimeReadCallbacks(value);
				return value;
			}
		}

		/// <inheritdoc cref="IDateTime.UnixEpoch" />
		public DateTime UnixEpoch
			=> _timeSystemMock.TimeProvider.UnixEpoch;

		/// <inheritdoc cref="IDateTime.UtcNow" />
		public DateTime UtcNow
		{
			get
			{
				DateTime value = _timeSystemMock.TimeProvider.Read().ToUniversalTime();
				_callbackHandler.InvokeDateTimeReadCallbacks(value);
				return value;
			}
		}

		#endregion
	}
}