using System;

namespace Testably.Abstractions.Testing.TimeSystem;

public class TimerExecution
{
	public DateTime Time { get; }
	public ITimerMock Timer { get; }

	internal TimerExecution(DateTime time, ITimerMock timer)
	{
		Time = time;
		Timer = timer;
	}
}
