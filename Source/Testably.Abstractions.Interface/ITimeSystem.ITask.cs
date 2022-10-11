using System;
using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions;

public partial interface ITimeSystem
{
	/// <summary>
	///     Abstractions for <see cref="System.Threading.Tasks.Task" />.
	/// </summary>
	interface ITask : ITimeSystemExtensionPoint
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
}