using System;
using System.Threading;
using System.Threading.Tasks;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Testing.TimeSystem;

internal sealed class TaskMock : ITask
{
	private readonly bool _autoAdvance;
	private readonly NotificationHandler _callbackHandler;
	private readonly MockTimeSystem _mockTimeSystem;

	internal TaskMock(MockTimeSystem timeSystem,
		NotificationHandler callbackHandler,
		bool autoAdvance)
	{
		_mockTimeSystem = timeSystem;
		_callbackHandler = callbackHandler;
		_autoAdvance = autoAdvance;
	}

	#region ITask Members

	/// <inheritdoc cref="ITimeSystemEntity.TimeSystem" />
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

		if (_autoAdvance)
		{
			_mockTimeSystem.TimeProvider.AdvanceBy(delay);
			_callbackHandler.InvokeTaskDelayCallbacks(delay);
			return Task.CompletedTask;
		}

		_callbackHandler.InvokeTaskDelayCallbacks(delay);

		if (delay == TimeSpan.Zero)
		{
			return Task.CompletedTask;
		}

		return DelayUntilAdvanced(delay, cancellationToken);
	}

	#endregion

	/// <summary>
	///     Returns a task that stays pending until the simulated clock advances to or past
	///     <c>now + <paramref name="delay" /></c>, or faults with an
	///     <see cref="OperationCanceledException" /> if the <paramref name="cancellationToken" /> is
	///     cancelled while pending.
	///     <para />
	///     A <see cref="Timeout.InfiniteTimeSpan" /> never completes on its own and can only be
	///     released via cancellation.
	/// </summary>
	private async Task DelayUntilAdvanced(TimeSpan delay, CancellationToken cancellationToken)
	{
		bool isInfinite = delay.TotalMilliseconds < 0;
		long targetTicks = _mockTimeSystem.TimeProvider.ElapsedTicks + delay.Ticks;

		while (true)
		{
			using IAwaitableCallback<DateTime> onTimeChanged = _mockTimeSystem.On
				.TimeChanged(predicate: _
					=> !isInfinite &&
					   _mockTimeSystem.TimeProvider.ElapsedTicks >= targetTicks);
			if (!isInfinite &&
			    _mockTimeSystem.TimeProvider.ElapsedTicks >= targetTicks)
			{
				return;
			}

			await onTimeChanged.WaitAsync(
				timeout: null,
				cancellationToken: cancellationToken).ConfigureAwait(false);
		}
	}
}
