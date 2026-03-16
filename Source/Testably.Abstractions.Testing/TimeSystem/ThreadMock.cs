using System;
using System.Threading;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Testing.TimeSystem;

internal sealed class ThreadMock : IThread
{
	private readonly bool _autoAdvance;
	private readonly NotificationHandler _callbackHandler;
	private readonly MockTimeSystem _mockTimeSystem;

	internal ThreadMock(MockTimeSystem timeSystem,
		NotificationHandler callbackHandler,
		bool autoAdvance)
	{
		_mockTimeSystem = timeSystem;
		_callbackHandler = callbackHandler;
		_autoAdvance = autoAdvance;
	}

	#region IThread Members

	/// <inheritdoc cref="ITimeSystemEntity.TimeSystem" />
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

		if (_autoAdvance)
		{
			_mockTimeSystem.TimeProvider.AdvanceBy(timeout);
		}

		Thread.Yield();
		_callbackHandler.InvokeThreadSleepCallbacks(timeout);
	}

	#endregion
}
