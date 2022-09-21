using System;

namespace Testably.Abstractions.Testing;

/// <summary>
///     A test helper for simulating the system time. Implements <see cref="ITimeSystem" />.
///     <para />
///     The <see cref="TimeProvider" /> allows manipulating the simulated system time.
/// </summary>
public sealed partial class TimeSystemMock : ITimeSystem
{
    /// <summary>
    ///     The callback handler for the <see cref="TimeSystemMock" />
    /// </summary>
    public ICallbackHandler On
        => _callbackHandler;

    /// <summary>
    ///     The time provider for the currently simulated system time.
    /// </summary>
    public ITimeProvider TimeProvider { get; }

    private readonly TimeSystemMockCallbackHandler _callbackHandler;
    private readonly DateTimeMock _dateTimeMock;
    private readonly TaskMock _taskMock;
    private readonly ThreadMock _threadMock;

    /// <summary>
    ///     Initializes the <see cref="TimeSystemMock" /> with a random time.
    /// </summary>
    public TimeSystemMock() : this(Testing.TimeProvider.Random())
    {
    }

    /// <summary>
    ///     Initializes the <see cref="TimeSystemMock" /> with the specified <paramref name="time" />.
    /// </summary>
    public TimeSystemMock(DateTime time) : this(Testing.TimeProvider.Set(time))
    {
    }

    /// <summary>
    ///     Initializes the <see cref="TimeSystemMock" /> with the specified <paramref name="timeProviderProvider" />.
    /// </summary>
    public TimeSystemMock(ITimeProvider timeProviderProvider)
    {
        TimeProvider = timeProviderProvider;
        _callbackHandler = new TimeSystemMockCallbackHandler();
        _dateTimeMock = new DateTimeMock(this, _callbackHandler);
        _threadMock = new ThreadMock(this, _callbackHandler);
        _taskMock = new TaskMock(this, _callbackHandler);
    }

    #region ITimeSystem Members

    /// <inheritdoc cref="ITimeSystem.DateTime" />
    public ITimeSystem.IDateTime DateTime
        => _dateTimeMock;

    /// <inheritdoc cref="ITimeSystem.Task" />
    public ITimeSystem.ITask Task
        => _taskMock;

    /// <inheritdoc cref="ITimeSystem.Thread" />
    public ITimeSystem.IThread Thread
        => _threadMock;

    #endregion
}