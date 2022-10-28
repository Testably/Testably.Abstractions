using System;
using System.Threading;
using System.Threading.Tasks;
using Testably.Abstractions.Testing.Internal;
using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Testing.TimeSystem;

internal sealed class TaskMock : ITask
{
	private readonly TimeSystemMockCallbackHandler _callbackHandler;
	private readonly MockTimeSystem _mockTimeSystem;

	internal TaskMock(MockTimeSystem timeSystem,
					  TimeSystemMockCallbackHandler callbackHandler)
	{
		_mockTimeSystem = timeSystem;
		_callbackHandler = callbackHandler;
	}

	#region ITask Members

	/// <inheritdoc cref="ITimeSystemExtensionPoint.TimeSystem" />
	public ITimeSystem TimeSystem
		=> _mockTimeSystem;

	public Task Delay(int millisecondsDelay)
		=> Delay(millisecondsDelay, CancellationToken.None);

	public Task Delay(int millisecondsDelay, CancellationToken cancellationToken)
	{
		if (millisecondsDelay < -1)
		{
			throw ExceptionFactory.TaskDelayOutOfRange(nameof(millisecondsDelay));
		}

		return Delay(TimeSpan.FromMilliseconds(millisecondsDelay),
			cancellationToken);
	}

	public Task Delay(TimeSpan delay)
		=> Delay(delay, CancellationToken.None);

	public Task Delay(TimeSpan delay, CancellationToken cancellationToken)
	{
		if (delay.TotalMilliseconds < -1)
		{
			throw ExceptionFactory.TaskDelayOutOfRange(nameof(delay));
		}

		if (cancellationToken.IsCancellationRequested)
		{
			throw ExceptionFactory.TaskWasCanceled();
		}

		_mockTimeSystem.TimeProvider.AdvanceBy(delay);
		_callbackHandler.InvokeTaskDelayCallbacks(delay);
		return Task.CompletedTask;
	}

	#endregion
}