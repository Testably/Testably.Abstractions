using System;
using Testably.Abstractions.Testing.Internal;

namespace Testably.Abstractions.Testing;

public sealed partial class TimeSystemMock
{
    private sealed class ThreadMock : ITimeSystem.IThread
    {
        private readonly TimeSystemMockCallbackHandler _callbackHandler;
        private readonly TimeSystemMock _timeSystemMock;

        internal ThreadMock(TimeSystemMock timeSystem,
                            TimeSystemMockCallbackHandler callbackHandler)
        {
            _timeSystemMock = timeSystem;
            _callbackHandler = callbackHandler;
        }

        #region IThread Members

        /// <inheritdoc cref="ITimeSystem.ITimeSystemExtensionPoint.TimeSystem" />
        public ITimeSystem TimeSystem
            => _timeSystemMock;

        public void Sleep(int millisecondsTimeout)
        {
            if (millisecondsTimeout < -1)
            {
                throw ExceptionFactory.ThreadSleepOutOfRange(nameof(millisecondsTimeout));
            }

            Sleep(TimeSpan.FromMilliseconds(millisecondsTimeout));
        }

        public void Sleep(TimeSpan timeout)
        {
            if (timeout.TotalMilliseconds < -1)
            {
                throw ExceptionFactory.ThreadSleepOutOfRange(nameof(timeout));
            }

            _timeSystemMock.TimeProvider.AdvanceBy(timeout);
            System.Threading.Thread.Sleep(0);
            _callbackHandler.InvokeThreadSleepCallbacks(timeout);
        }

        #endregion
    }
}