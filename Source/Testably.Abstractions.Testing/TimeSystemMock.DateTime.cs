﻿using System;
using Testably.Abstractions.Testing.Internal;

namespace Testably.Abstractions.Testing;

public sealed partial class TimeSystemMock
{
    private sealed class DateTimeMock : ITimeSystem.IDateTime
    {
        private readonly TimeSystemMockCallbackHandler _callbackHandler;
        private readonly TimeSystemMock _timeSystemMock;

        internal DateTimeMock(TimeSystemMock timeSystem,
                              TimeSystemMockCallbackHandler callbackHandler)
        {
            _timeSystemMock = timeSystem;
            _callbackHandler = callbackHandler;
        }

        #region IDateTime Members

        /// <inheritdoc cref="ITimeSystem.IDateTime.MaxValue" />
        public DateTime MaxValue
            => _timeSystemMock.TimeProvider.MaxValue;

        /// <inheritdoc cref="ITimeSystem.IDateTime.MinValue" />
        public DateTime MinValue
            => _timeSystemMock.TimeProvider.MinValue;

        /// <inheritdoc cref="ITimeSystem.IDateTime.Now" />
        public DateTime Now
        {
            get
            {
                DateTime value = _timeSystemMock.TimeProvider.Read().ToLocalTime();
                _callbackHandler.InvokeDateTimeReadCallbacks(value);
                return value;
            }
        }

        /// <inheritdoc cref="ITimeSystem.ITimeSystemExtensionPoint.TimeSystem" />
        public ITimeSystem TimeSystem
            => _timeSystemMock;

        /// <inheritdoc cref="ITimeSystem.IDateTime.Today" />
        public DateTime Today
        {
            get
            {
                DateTime value = _timeSystemMock.TimeProvider.Read().Date;
                _callbackHandler.InvokeDateTimeReadCallbacks(value);
                return value;
            }
        }

        /// <inheritdoc cref="ITimeSystem.IDateTime.UnixEpoch" />
        public DateTime UnixEpoch
            => _timeSystemMock.TimeProvider.UnixEpoch;

        /// <inheritdoc cref="ITimeSystem.IDateTime.UtcNow" />
        public DateTime UtcNow
        {
            get
            {
                DateTime value = _timeSystemMock.TimeProvider.Read().ToUniversalTime();
                _callbackHandler.InvokeDateTimeReadCallbacks(value);
                return value;
            }
        }

        #endregion
    }
}