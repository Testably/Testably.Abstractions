using System;
using System.Threading;
using System.Threading.Tasks;
using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions;

public sealed partial class RealTimeSystem
{
	private sealed class TaskWrapper : ITask
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
			return System.Threading.Tasks.Task.Delay(millisecondsDelay);
		}

		/// <inheritdoc cref="ITask.Delay(int, CancellationToken)" />
		public Task Delay(int millisecondsDelay, CancellationToken cancellationToken)
		{
			return System.Threading.Tasks.Task.Delay(millisecondsDelay,
				cancellationToken);
		}

		/// <inheritdoc cref="ITask.Delay(TimeSpan)" />
		public Task Delay(TimeSpan delay)
		{
			return System.Threading.Tasks.Task.Delay(delay);
		}

		/// <inheritdoc cref="ITask.Delay(TimeSpan, CancellationToken)" />
		public Task Delay(TimeSpan delay, CancellationToken cancellationToken)
		{
			return System.Threading.Tasks.Task.Delay(delay, cancellationToken);
		}

		#endregion
	}
}