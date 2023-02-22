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

	internal TimerExecution(DateTime time, ITimerMock timer)
	{
		Time = time;
		Timer = timer;
	}
}
