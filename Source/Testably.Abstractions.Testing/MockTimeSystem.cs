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

	/// <summary>
	///     The schedule of the timers, which moves the <see cref="TimeProvider" /> from one due time to the next.
	/// </summary>
	internal TimerScheduler Scheduler { get; }

	private readonly NotificationHandler _callbackHandler;
	private readonly DateTimeMock _dateTimeMock;
	private readonly DateTimeOffsetMock _dateTimeOffsetMock;
	private readonly TimeZoneInfoMock _timeZoneInfoMock;
	private readonly StopwatchFactoryMock _stopwatchFactoryMock;
	private readonly TaskMock _taskMock;
	private readonly ThreadMock _threadMock;
#if FEATURE_PERIODIC_TIMER
	private readonly PeriodicTimerFactoryMock _periodicTimerFactoryMock;
#endif
	private readonly TimerFactoryMock _timerFactoryMock;

	/// <summary>
	///     Initializes the <see cref="MockTimeSystem" /> with a random time and the local time zone
	///     (<see cref="TimeZoneInfo.Local" />).
	/// </summary>
	public MockTimeSystem() : this(TimeProviderFactory.Random(System.TimeZoneInfo.Local), options => options)
	{
	}

	/// <summary>
	///     Initializes the <see cref="MockTimeSystem" /> with a random time and the local time zone
	///     (<see cref="TimeZoneInfo.Local" />).
	/// </summary>
	public MockTimeSystem(Func<MockTimeSystemOptions, MockTimeSystemOptions> options) : this(TimeProviderFactory.Random(System.TimeZoneInfo.Local), options)
	{
	}

	/// <summary>
	///     Initializes the <see cref="MockTimeSystem" /> with the specified <paramref name="time" />.
	/// </summary>
	public MockTimeSystem(DateTime time) : this(TimeProviderFactory.Use(time), options => options)
	{
	}

	/// <summary>
	///     Initializes the <see cref="MockTimeSystem" /> with the specified <paramref name="time" />.
	/// </summary>
	public MockTimeSystem(DateTime time, Func<MockTimeSystemOptions, MockTimeSystemOptions> options) : this(TimeProviderFactory.Use(time), options)
	{
	}

	/// <summary>
	///     Initializes the <see cref="MockTimeSystem" /> with the specified <paramref name="timeProvider" />.
	/// </summary>
	[Obsolete("Use the constructor with ITimeProviderFactory instead.")]
	public MockTimeSystem(ITimeProvider timeProvider) : this(
		new TimeProviderFactory.Factory(_ => timeProvider))
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
		TimerScheduler? scheduler = null;
		TimeProvider = timeProvider.Create(now =>
		{
			scheduler?.OnTimeChanged();
			_callbackHandler.InvokeTimeChanged(now);
		});
		scheduler = new TimerScheduler(TimeProvider, initialization.StrictCallbackTimeout);
		(TimeProvider as TimeProviderMock)?.UseScheduler(scheduler);
		Scheduler = scheduler;
		_dateTimeMock = new DateTimeMock(this, _callbackHandler);
		_dateTimeOffsetMock = new DateTimeOffsetMock(this, _callbackHandler);
		_timeZoneInfoMock = new TimeZoneInfoMock(this);
		_stopwatchFactoryMock = new StopwatchFactoryMock(this);
		_threadMock = new ThreadMock(this, _callbackHandler, initialization.AutoAdvance);
		_taskMock = new TaskMock(this, _callbackHandler, initialization.AutoAdvance);
#if FEATURE_PERIODIC_TIMER
		_periodicTimerFactoryMock = new PeriodicTimerFactoryMock(this, _callbackHandler, initialization.AutoAdvance);
#endif
		_timerFactoryMock = new TimerFactoryMock(this, initialization.AutoAdvance);
	}

	#region ITimeSystem Members

	/// <inheritdoc cref="ITimeSystem.DateTime" />
	public IDateTime DateTime
		=> _dateTimeMock;

	/// <inheritdoc cref="ITimeSystem.DateTimeOffset" />
	public IDateTimeOffset DateTimeOffset
		=> _dateTimeOffsetMock;

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

	/// <inheritdoc cref="ITimeSystem.TimeZoneInfo" />
	public ITimeZoneInfo TimeZoneInfo
		=> _timeZoneInfoMock;

	#endregion

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> $"MockTimeSystem (provider: {TimeProvider}, now: {DateTime.UtcNow}Z)";

	/// <summary>
	///     Specifies the <see cref="TimerStrategy" /> to use when dealing with timers.
	/// </summary>
	/// <param name="timerStrategy">The timer strategy. </param>
	public MockTimeSystem WithTimerStrategy(TimerStrategy timerStrategy)
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
		///     How long the clock waits for a blocked timer callback with a strict timer order, or <see langword="null" />
		///     without it.
		/// </summary>
		internal TimeSpan? StrictCallbackTimeout { get; private set; }

		/// <summary>
		///     Disables automatic advancement of the time when waiting for timers, tasks or threads to complete.
		/// </summary>
		public MockTimeSystemOptions DisableAutoAdvance()
		{
			AutoAdvance = false;
			return this;
		}

		/// <summary>
		///     Lets the thread that moves the clock wait for each timer callback until it completes, so that the timers
		///     fire in the order of their due time also on a loaded machine. The clock waits for a blocked callback at most
		///     5 seconds.
		/// </summary>
		/// <remarks>
		///     Enable it when your tests depend on the order of the timers, e.g. a timeout that must cancel a delay before
		///     the delay completes, and fail only on a loaded machine, like a busy CI runner.
		///     <para />
		///     By default, the clock moves on when a callback blocked for one second of real time, as it may wait for the
		///     clock. With a strict timer order, the clock waits until the callback completes or waits for the clock
		///     through the <see cref="MockTimeSystem" />. When that takes longer than the timeout, the scheduler fails: the
		///     code that moves the clock throws a <see cref="TimeoutException" />, and so does every later move of the clock
		///     and every wait for idle timers. Another thread that moves the clock at the same time can still move it past
		///     a running callback. The timers that move the clock on their own also wait up to the timeout, instead of one
		///     second, for the creator of a timer to set it up, e.g. for a delay to register its cancellation, and then
		///     move on, like the real clock does.
		/// </remarks>
		public MockTimeSystemOptions EnableStrictTimerOrder()
			=> EnableStrictTimerOrder(TimeSpan.FromSeconds(5));

		/// <summary>
		///     Lets the thread that moves the clock wait for each timer callback until it completes, so that the timers
		///     fire in the order of their due time also on a loaded machine. The clock waits for a blocked callback at most
		///     the <paramref name="callbackTimeout" />.
		/// </summary>
		/// <remarks>
		///     Enable it when your tests depend on the order of the timers, e.g. a timeout that must cancel a delay before
		///     the delay completes, and fail only on a loaded machine, like a busy CI runner.
		///     <para />
		///     By default, the clock moves on when a callback blocked for one second of real time, as it may wait for the
		///     clock. With a strict timer order, the clock waits until the callback completes or waits for the clock
		///     through the <see cref="MockTimeSystem" />. When that takes longer than the
		///     <paramref name="callbackTimeout" />, the scheduler fails: the code that moves the clock throws a
		///     <see cref="TimeoutException" />, and so does every later move of the clock and every wait for idle timers.
		///     Another thread that moves the clock at the same time can still move it past a running callback. The timers
		///     that move the clock on their own also wait up to the <paramref name="callbackTimeout" />, instead of one
		///     second, for the creator of a timer to set it up, e.g. for a delay to register its cancellation, and then
		///     move on, like the real clock does.
		/// </remarks>
		/// <param name="callbackTimeout">
		///     How long the clock waits for a blocked callback, or <see cref="System.Threading.Timeout.InfiniteTimeSpan" />
		///     to wait forever.
		/// </param>
		public MockTimeSystemOptions EnableStrictTimerOrder(TimeSpan callbackTimeout)
		{
			if ((callbackTimeout <= TimeSpan.Zero && callbackTimeout != System.Threading.Timeout.InfiniteTimeSpan) ||
			    callbackTimeout.TotalMilliseconds > int.MaxValue)
			{
				throw new ArgumentOutOfRangeException(nameof(callbackTimeout));
			}

			StrictCallbackTimeout = callbackTimeout;
			return this;
		}
	}
}
