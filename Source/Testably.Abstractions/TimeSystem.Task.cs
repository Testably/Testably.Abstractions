using System;
using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions;

public sealed partial class TimeSystem
{
    private sealed class TaskSystem : ITimeSystem.ITask
    {
        internal TaskSystem(TimeSystem timeSystem)
        {
            TimeSystem = timeSystem;
        }

        #region ITask Members

        /// <inheritdoc cref="ITimeSystem.ITimeSystemExtensionPoint.TimeSystem" />
        public ITimeSystem TimeSystem { get; }

        /// <inheritdoc cref="ITimeSystem.ITask.Delay(int)" />
        public Task Delay(int millisecondsDelay)
        {
            return System.Threading.Tasks.Task.Delay(millisecondsDelay);
        }

        /// <inheritdoc cref="ITimeSystem.ITask.Delay(int, CancellationToken)" />
        public Task Delay(int millisecondsDelay, CancellationToken cancellationToken)
        {
            return System.Threading.Tasks.Task.Delay(millisecondsDelay,
                cancellationToken);
        }

        /// <inheritdoc cref="ITimeSystem.ITask.Delay(TimeSpan)" />
        public Task Delay(TimeSpan delay)
        {
            return System.Threading.Tasks.Task.Delay(delay);
        }

        /// <inheritdoc cref="ITimeSystem.ITask.Delay(TimeSpan, CancellationToken)" />
        public Task Delay(TimeSpan delay, CancellationToken cancellationToken)
        {
            return System.Threading.Tasks.Task.Delay(delay, cancellationToken);
        }

        #endregion
    }
}