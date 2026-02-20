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
	public ITimerHandler TimerHandler => _timerFactoryMock;

	private readonly NotificationHandler _callbackHandler;
	private readonly DateTimeMock _dateTimeMock;
	private readonly StopwatchFactoryMock _stopwatchFactoryMock;
	private readonly TaskMock _taskMock;
	private readonly ThreadMock _threadMock;
	private readonly TimerFactoryMock _timerFactoryMock;

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
		_stopwatchFactoryMock = new StopwatchFactoryMock(this);
		_threadMock = new ThreadMock(this, _callbackHandler);
		_taskMock = new TaskMock(this, _callbackHandler);
		_timerFactoryMock = new TimerFactoryMock(this);
	}

	#region ITimeSystem Members

	/// <inheritdoc cref="ITimeSystem.DateTime" />
	public IDateTime DateTime
		=> _dateTimeMock;

	/// <inheritdoc cref="ITimeSystem.Stopwatch" />
	public IStopwatchFactory Stopwatch
		=> _stopwatchFactoryMock;

	/// <inheritdoc cref="ITimeSystem.Task" />
	public ITask Task
		=> _taskMock;

	/// <inheritdoc cref="ITimeSystem.Thread" />
	public IThread Thread
		=> _threadMock;

	/// <inheritdoc cref="ITimeSystem.Timer" />
	public ITimerFactory Timer
		=> _timerFactoryMock;

	#endregion

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> $"MockTimeSystem (provider: {TimeProvider}, now: {DateTime.UtcNow}Z)";

	/// <summary>
	///     Specifies the <see cref="ITimerStrategy" /> to use when dealing with timers.
	/// </summary>
	/// <param name="timerStrategy">The timer strategy. </param>
	public MockTimeSystem WithTimerStrategy(ITimerStrategy timerStrategy)
	{
		_timerFactoryMock.SetTimerStrategy(timerStrategy);
		return this;
	}
}
