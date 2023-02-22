namespace Testably.Abstractions.Testing.TimeSystem;

public class TimerStrategy : ITimerStrategy
{
	public static ITimerStrategy Default { get; }
		= new TimerStrategy(TimerMode.StartImmediately);

	public TimerMode Mode { get; }

	public TimerStrategy(TimerMode mode)
	{
		Mode = mode;
	}
}
