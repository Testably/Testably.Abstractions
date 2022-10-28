using System;
using Testably.Abstractions.Testing.Internal;

namespace Testably.Abstractions.Testing;

public sealed partial class MockTimeSystem
{
	private sealed class ThreadMock : IThread
	{
		private readonly TimeSystemMockCallbackHandler _callbackHandler;
		private readonly MockTimeSystem _timeSystemMock;

		internal ThreadMock(MockTimeSystem timeSystem,
		                    TimeSystemMockCallbackHandler callbackHandler)
		{
			_timeSystemMock = timeSystem;
			_callbackHandler = callbackHandler;
		}

		#region IThread Members

		/// <inheritdoc cref="ITimeSystemExtensionPoint.TimeSystem" />
		public ITimeSystem TimeSystem
			=> _timeSystemMock;

		public void Sleep(int millisecondsTimeout)
			=> Sleep(TimeSpan.FromMilliseconds(millisecondsTimeout));

		public void Sleep(TimeSpan timeout)
		{
			if (timeout.TotalMilliseconds < -1)
			{
				throw ExceptionFactory.ThreadSleepOutOfRange(nameof(timeout));
			}

			_timeSystemMock.TimeProvider.AdvanceBy(timeout);
			System.Threading.Thread.Sleep(0);
			_callbackHandler.InvokeThreadSleepCallbacks(timeout);
		}

		#endregion
	}
}