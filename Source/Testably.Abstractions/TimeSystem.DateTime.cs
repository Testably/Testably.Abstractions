using System;

namespace Testably.Abstractions;

public sealed partial class TimeSystem
{
    private sealed class DateTimeSystem : ITimeSystem.IDateTime
    {
        internal DateTimeSystem(TimeSystem timeSystem)
        {
            TimeSystem = timeSystem;
        }

        #region IDateTime Members

        /// <inheritdoc cref="ITimeSystem.IDateTime.MaxValue" />
        public DateTime MaxValue => System.DateTime.MaxValue;

        /// <inheritdoc cref="ITimeSystem.IDateTime.MinValue" />
        public DateTime MinValue => System.DateTime.MinValue;

#if NETSTANDARD2_0
        /// <inheritdoc cref="ITimeSystem.IDateTime.UnixEpoch" />
        public DateTime UnixEpoch => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
#else
        /// <inheritdoc cref="ITimeSystem.IDateTime.UnixEpoch" />
        public DateTime UnixEpoch => System.DateTime.UnixEpoch;
#endif

        /// <inheritdoc cref="ITimeSystem.IDateTime.Now" />
        public DateTime Now => System.DateTime.Now;

        /// <inheritdoc cref="ITimeSystem.IDateTime.UtcNow" />
        public DateTime UtcNow => System.DateTime.UtcNow;

        /// <inheritdoc cref="ITimeSystem.IDateTime.Today" />
        public DateTime Today => System.DateTime.Today;

        /// <inheritdoc cref="ITimeSystem.ITimeSystemExtensionPoint.TimeSystem" />
        public ITimeSystem TimeSystem { get; }

        #endregion
    }
}