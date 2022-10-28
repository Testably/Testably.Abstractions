using System;
using Testably.Abstractions.Testing.Internal;
using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Testing;

public sealed partial class MockTimeSystem
{
	private sealed class ThreadMock : IThread
	{
		private readonly TimeSystemMockCallbackHandler _callbackHandler;
		private readonly MockTimeSystem _mockTimeSystem;

		internal ThreadMock(MockTimeSystem timeSystem,
		                    TimeSystemMockCallbackHandler callbackHandler)
		{
			_mockTimeSystem = timeSystem;
			_callbackHandler = callbackHandler;
		}

		#region IThread Members

		/// <inheritdoc cref="ITimeSystemExtensionPoint.TimeSystem" />
		public ITimeSystem TimeSystem
			=> _mockTimeSystem;

		public void Sleep(int millisecondsTimeout)
			=> Sleep(TimeSpan.FromMilliseconds(millisecondsTimeout));

		public void Sleep(TimeSpan timeout)
		{
			if (timeout.TotalMilliseconds < -1)
			{
				throw ExceptionFactory.ThreadSleepOutOfRange(nameof(timeout));
			}

			_mockTimeSystem.TimeProvider.AdvanceBy(timeout);
			System.Threading.Thread.Sleep(0);
			_callbackHandler.InvokeThreadSleepCallbacks(timeout);
		}

		#endregion
	}
}