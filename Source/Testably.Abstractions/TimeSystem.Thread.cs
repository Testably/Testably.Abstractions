using System;

namespace Testably.Abstractions;

public sealed partial class TimeSystem
{
    private sealed class ThreadSystem : ITimeSystem.IThread
    {
        internal ThreadSystem(TimeSystem timeSystem)
        {
            TimeSystem = timeSystem;
        }

        #region IThread Members

        /// <inheritdoc cref="ITimeSystem.IThread.Sleep(int)" />
        public void Sleep(int millisecondsTimeout)
        {
            System.Threading.Thread.Sleep(millisecondsTimeout);
        }

        /// <inheritdoc cref="ITimeSystem.IThread.Sleep(TimeSpan)" />
        public void Sleep(TimeSpan timeout)
        {
            System.Threading.Thread.Sleep(timeout);
        }

        /// <inheritdoc cref="ITimeSystem.ITimeSystemExtensionPoint.TimeSystem" />
        public ITimeSystem TimeSystem { get; }

        #endregion
    }
}