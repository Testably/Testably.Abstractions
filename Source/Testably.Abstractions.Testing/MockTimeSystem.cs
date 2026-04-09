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
#if FEATURE_PERIODIC_TIMER
	private readonly PeriodicTimerFactoryMock _periodicTimerFactoryMock;
#endif
	private readonly TimerFactoryMock _timerFactoryMock;

	/// <summary>
	///     Initializes the <see cref="MockTimeSystem" /> with a random time.
	/// </summary>
	public MockTimeSystem() : this(Testing.TimeProvider.Random(), options => options)
	{
	}

	/// <summary>
	///     Initializes the <see cref="MockTimeSystem" /> with a random time.
	/// </summary>
	public MockTimeSystem(Func<MockTimeSystemOptions, MockTimeSystemOptions> options) : this(Testing.TimeProvider.Random(), options)
	{
	}

	/// <summary>
	///     Initializes the <see cref="MockTimeSystem" /> with the specified <paramref name="time" />.
	/// </summary>
	public MockTimeSystem(DateTime time) : this(Testing.TimeProvider.Use(time), options => options)
	{
	}

	/// <summary>
	///     Initializes the <see cref="MockTimeSystem" /> with the specified <paramref name="time" />.
	/// </summary>
	public MockTimeSystem(DateTime time, Func<MockTimeSystemOptions, MockTimeSystemOptions> options) : this(Testing.TimeProvider.Use(time), options)
	{
	}

	/// <summary>
	///     Initializes the <see cref="MockTimeSystem" /> with the specified <paramref name="timeProvider" />.
	/// </summary>
	[Obsolete("Use the constructor with ITimeProviderFactory instead.")]
	public MockTimeSystem(ITimeProvider timeProvider) : this(
		new TimeProvider.Factory(_ => timeProvider))
	{
	}

	/// <summary>
	///     Initializes the <see cref="MockTimeSystem" /> with the specified <paramref name="timeProvider" />.
	/// </summary>
	public MockTimeSystem(ITimeProviderFactory timeProvider) : this(timeProvider, options => options)
	{
	}

	/// <summary>
	///     Initializes the <see cref="MockTimeSystem" /> with the specified <paramref name="timeProvider" /> and the given <paramref name="options" />.
	/// </summary>
	public MockTimeSystem(ITimeProviderFactory timeProvider, Func<MockTimeSystemOptions, MockTimeSystemOptions> options)
	{
		MockTimeSystemOptions initialization = new();
		initialization = options(initialization);
		
		_callbackHandler = new NotificationHandler(this);
		TimeProvider = timeProvider.Create(_callbackHandler.InvokeTimeChanged);
		_dateTimeMock = new DateTimeMock(this, _callbackHandler);
		_stopwatchFactoryMock = new StopwatchFactoryMock(this);
		_threadMock = new ThreadMock(this, _callbackHandler, initialization.AutoAdvance);
		_taskMock = new TaskMock(this, _callbackHandler, initialization.AutoAdvance);
#if FEATURE_PERIODIC_TIMER
		_periodicTimerFactoryMock = new PeriodicTimerFactoryMock(this, _callbackHandler, initialization.AutoAdvance);
#endif
		_timerFactoryMock = new TimerFactoryMock(this);
	}

	#region ITimeSystem Members

	/// <inheritdoc cref="ITimeSystem.DateTime" />
	public IDateTime DateTime
		=> _dateTimeMock;

#if FEATURE_PERIODIC_TIMER
	/// <inheritdoc cref="ITimeSystem.PeriodicTimer" />
	public IPeriodicTimerFactory PeriodicTimer
		=> _periodicTimerFactoryMock;
#endif

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

	/// <summary>
	///     The initialization options for the <see cref="MockTimeSystem" />.
	/// </summary>
	public class MockTimeSystemOptions
	{
		/// <summary>
		///     Flag indicating if the time should automatically be advanced when waiting for timers, tasks or threads to complete.
		/// </summary>
		internal bool AutoAdvance { get; private set; } = true;

		/// <summary>
		///     Disables automatic advancement of the time when waiting for timers, tasks or threads to complete.
		/// </summary>
		public MockTimeSystemOptions DisableAutoAdvance()
		{
			AutoAdvance = false;
			return this;
		}
	}
}
