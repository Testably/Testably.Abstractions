using System;
using System.Threading;
using System.Threading.Tasks;
using Testably.Abstractions.Testing.Internal;

namespace Testably.Abstractions.Testing;

public sealed partial class TimeSystemMock
{
    private sealed class TaskMock : ITimeSystem.ITask
    {
        private readonly TimeSystemMock _timeSystemMock;
        private readonly TimeSystemMockCallbackHandler _callbackHandler;

        internal TaskMock(TimeSystemMock timeSystem,
                          TimeSystemMockCallbackHandler callbackHandler)
        {
            _timeSystemMock = timeSystem;
            _callbackHandler = callbackHandler;
        }

        #region ITask Members

        public Task Delay(int millisecondsDelay)
        {
            return Delay(millisecondsDelay, CancellationToken.None);
        }

        public Task Delay(int millisecondsDelay, CancellationToken cancellationToken)
        {
            if (millisecondsDelay < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(millisecondsDelay),
                    "The value needs to be either -1 (signifying an infinite timeout), 0 or a positive integer.");
            }

            return Delay(TimeSpan.FromMilliseconds(millisecondsDelay),
                cancellationToken);
        }

        public Task Delay(TimeSpan delay)
        {
            return Delay(delay, CancellationToken.None);
        }

        public Task Delay(TimeSpan delay, CancellationToken cancellationToken)
        {
            if (delay.TotalMilliseconds < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(delay),
                    "The value needs to be either -1 (signifying an infinite timeout), 0 or a positive integer.");
            }

            if (cancellationToken.IsCancellationRequested)
            {
                throw new TaskCanceledException("A task was canceled.");
            }

            _timeSystemMock.TimeProvider.AdvanceBy(delay);
            _callbackHandler.InvokeTaskDelayCallbacks(delay);
            return System.Threading.Tasks.Task.CompletedTask;
        }

        /// <inheritdoc cref="ITimeSystem.ITimeSystemExtensionPoint.TimeSystem" />
        public ITimeSystem TimeSystem
            => _timeSystemMock;

        #endregion
    }
}