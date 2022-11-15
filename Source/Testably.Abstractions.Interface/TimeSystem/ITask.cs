using System;
using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.TimeSystem;

/// <summary>
///     Abstractions for <see cref="Task" />.
/// </summary>
public interface ITask : ITimeSystemEntity
{
	/// <inheritdoc cref="Task.Delay(int)" />
	Task Delay(int millisecondsDelay);

	/// <inheritdoc cref="Task.Delay(int, CancellationToken)" />
	Task Delay(int millisecondsDelay, CancellationToken cancellationToken);

	/// <inheritdoc cref="Task.Delay(TimeSpan)" />
	Task Delay(TimeSpan delay);

	/// <inheritdoc cref="Task.Delay(TimeSpan, CancellationToken)" />
	Task Delay(TimeSpan delay, CancellationToken cancellationToken);
}
