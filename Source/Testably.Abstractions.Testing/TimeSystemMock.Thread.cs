using System;
using Testably.Abstractions.Testing.Internal;

namespace Testably.Abstractions.Testing;

public sealed partial class TimeSystemMock
{
    private sealed class ThreadMock : ITimeSystem.IThread
    {
        private readonly TimeSystemMock _timeSystemMock;
        private readonly TimeSystemMockCallbackHandler _callbackHandler;

        internal ThreadMock(TimeSystemMock timeSystem,
                            TimeSystemMockCallbackHandler callbackHandler)
        {
            _timeSystemMock = timeSystem;
            _callbackHandler = callbackHandler;
        }

        #region IThread Members

        public void Sleep(int millisecondsTimeout)
        {
            if (millisecondsTimeout < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout),
                    "Number must be either non-negative and less than or equal to Int32.MaxValue or -1.");
            }

            Sleep(TimeSpan.FromMilliseconds(millisecondsTimeout));
        }

        public void Sleep(TimeSpan timeout)
        {
            if (timeout.TotalMilliseconds < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout),
                    "Number must be either non-negative and less than or equal to Int32.MaxValue or -1.");
            }

            _timeSystemMock.TimeProvider.AdvanceBy(timeout);
            System.Threading.Thread.Sleep(0);
            _callbackHandler.InvokeThreadSleepCallbacks(timeout);
        }

        /// <inheritdoc cref="ITimeSystem.ITimeSystemExtensionPoint.TimeSystem" />
        public ITimeSystem TimeSystem
            => _timeSystemMock;

        #endregion
    }
}