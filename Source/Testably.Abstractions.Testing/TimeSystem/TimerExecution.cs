using System;

namespace Testably.Abstractions.Testing.TimeSystem;

/// <summary>
///     A container for notifying the execution of a timer callback.
/// </summary>
public class TimerExecution
{
	/// <summary>
	///     The time when the callback finished executing.
	/// </summary>
	public DateTime Time { get; }

	/// <summary>
	///     The mocked timer.
	/// </summary>
	public ITimerMock Timer { get; }

	/// <summary>
	///     The exception thrown during this timer execution.
	/// </summary>
	public Exception? Exception { get; }

	internal TimerExecution(DateTime time, ITimerMock timer, Exception? exception)
	{
		Time = time;
		Timer = timer;
		Exception = exception;
	}
}
