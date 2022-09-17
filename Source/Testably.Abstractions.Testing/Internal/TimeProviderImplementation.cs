using System;

namespace Testably.Abstractions.Testing.Internal;

internal sealed class TimeProviderImplementation : TimeSystemMock.ITimeProvider
{
    private DateTime _now;
    private readonly object _lock = new();

    public TimeProviderImplementation(DateTime now)
    {
        _now = now;
    }

    #region ITimeProvider Members

    /// <inheritdoc cref="TimeSystemMock.ITimeProvider.MaxValue" />
    public DateTime MaxValue { get; set; } = DateTime.MaxValue;

    /// <inheritdoc cref="TimeSystemMock.ITimeProvider.MinValue" />
    public DateTime MinValue { get; set; } = DateTime.MinValue;

#if NETSTANDARD2_0
    /// <inheritdoc cref="ITimeSystem.IDateTime.UnixEpoch" />
    public DateTime UnixEpoch { get; set; } =
        new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
#else
    /// <inheritdoc cref="TimeSystemMock.ITimeProvider.UnixEpoch" />
    public DateTime UnixEpoch { get; set; } = DateTime.UnixEpoch;
#endif

    /// <inheritdoc cref="TimeSystemMock.ITimeProvider.AdvanceBy(TimeSpan)" />
    public void AdvanceBy(TimeSpan interval)
    {
        lock (_lock)
        {
            _now = _now.Add(interval);
        }
    }

    /// <inheritdoc cref="TimeSystemMock.ITimeProvider.Read()" />
    public DateTime Read()
    {
        return _now;
    }

    /// <inheritdoc cref="TimeSystemMock.ITimeProvider.SetTo(DateTime)" />
    public void SetTo(DateTime value)
    {
        lock (_lock)
        {
            _now = value;
        }
    }

    #endregion
}