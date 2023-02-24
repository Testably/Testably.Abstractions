using System;
using Testably.Abstractions.Testing.TimeSystem;
using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Testing;

/// <summary>
///     A test helper for simulating the system time. Implements <see cref="ITimeSystem" />.
///     <para />
///     The <see cref="TimeProvider" /> allows manipulating the simulated system time.
/// </summary>
public sealed class MockTimeSystem : ITimeSystem
{
	/// <summary>
	///     The callback handler for the <see cref="MockTimeSystem" />
	/// </summary>
	public INotificationHandler On
		=> _callbackHandler;

	/// <summary>
	///     The time provider for the currently simulated system time.
	/// </summary>
	public ITimeProvider TimeProvider { get; }

	/// <summary>
	///     The handler for mocked timers.
	/// </summary>
	public ITimerHandler TimerHandler => _timerMock;

	private readonly NotificationHandler _callbackHandler;
	private readonly DateTimeMock _dateTimeMock;
	private readonly TaskMock _taskMock;
	private readonly ThreadMock _threadMock;
	private readonly TimerFactoryMock _timerMock;

	/// <summary>
	///     Initializes the <see cref="MockTimeSystem" /> with a random time.
	/// </summary>
	public MockTimeSystem() : this(Testing.TimeProvider.Random())
	{
	}

	/// <summary>
	///     Initializes the <see cref="MockTimeSystem" /> with the specified <paramref name="time" />.
	/// </summary>
	public MockTimeSystem(DateTime time) : this(Testing.TimeProvider.Use(time))
	{
	}

	/// <summary>
	///     Initializes the <see cref="MockTimeSystem" /> with the specified <paramref name="timeProvider" />.
	/// </summary>
	public MockTimeSystem(ITimeProvider timeProvider)
	{
		TimeProvider = timeProvider;
		_callbackHandler = new NotificationHandler();
		_dateTimeMock = new DateTimeMock(this, _callbackHandler);
		_threadMock = new ThreadMock(this, _callbackHandler);
		_taskMock = new TaskMock(this, _callbackHandler);
		_timerMock = new TimerFactoryMock(this, _callbackHandler);
	}

	#region ITimeSystem Members

	/// <inheritdoc cref="ITimeSystem.DateTime" />
	public IDateTime DateTime
		=> _dateTimeMock;

	/// <inheritdoc cref="ITimeSystem.Task" />
	public ITask Task
		=> _taskMock;

	/// <inheritdoc cref="ITimeSystem.Thread" />
	public IThread Thread
		=> _threadMock;

	/// <inheritdoc cref="ITimeSystem.Timer" />
	public ITimerFactory Timer
		=> _timerMock;

	#endregion

	/// <summary>
	///     Specifies the <see cref="ITimerStrategy" /> to use when dealing with timers.
	/// </summary>
	/// <param name="timerStrategy">The timer strategy. </param>
	public MockTimeSystem WithTimerStrategy(ITimerStrategy timerStrategy)
	{
		_timerMock.SetTimerStrategy(timerStrategy);
		return this;
	}
}
