using System;
using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Testing;

/// <summary>
///     A test helper for simulating the system time. Implements <see cref="ITimeSystem" />.
///     <para />
///     The <see cref="TimeProvider" /> allows manipulating the simulated system time.
/// </summary>
public sealed partial class MockTimeSystem : ITimeSystem
{
	/// <summary>
	///     The callback handler for the <see cref="MockTimeSystem" />
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
		_callbackHandler = new TimeSystemMockCallbackHandler();
		_dateTimeMock = new DateTimeMock(this, _callbackHandler);
		_threadMock = new ThreadMock(this, _callbackHandler);
		_taskMock = new TaskMock(this, _callbackHandler);
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

	#endregion
}