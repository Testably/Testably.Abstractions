using System;
using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.TimeSystem;

internal sealed class TaskWrapper : ITask
{
	internal TaskWrapper(RealTimeSystem timeSystem)
	{
		TimeSystem = timeSystem;
	}

	#region ITask Members

	/// <inheritdoc cref="ITimeSystemExtensionPoint.TimeSystem" />
	public ITimeSystem TimeSystem { get; }

	/// <inheritdoc cref="ITask.Delay(int)" />
	public Task Delay(int millisecondsDelay)
	{
		return Task.Delay(millisecondsDelay);
	}

	/// <inheritdoc cref="ITask.Delay(int, CancellationToken)" />
	public Task Delay(int millisecondsDelay, CancellationToken cancellationToken)
	{
		return Task.Delay(millisecondsDelay,
			cancellationToken);
	}

	/// <inheritdoc cref="ITask.Delay(TimeSpan)" />
	public Task Delay(TimeSpan delay)
	{
		return Task.Delay(delay);
	}

	/// <inheritdoc cref="ITask.Delay(TimeSpan, CancellationToken)" />
	public Task Delay(TimeSpan delay, CancellationToken cancellationToken)
	{
		return Task.Delay(delay, cancellationToken);
	}

	#endregion
}
