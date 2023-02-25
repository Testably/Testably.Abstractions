namespace Testably.Abstractions.Testing.TimeSystem;

/// <summary>
///     The timer strategy.
/// </summary>
public class TimerStrategy : ITimerStrategy
{
	/// <summary>
	///     The default time strategy uses <see cref="TimerMode.StartImmediately" />.
	/// </summary>
	public static ITimerStrategy Default { get; }
		= new TimerStrategy(TimerMode.StartImmediately);

	/// <summary>
	///     The timer mode.
	/// </summary>
	public TimerMode Mode { get; }

	/// <summary>
	///     Initializes a new instance of <see cref="TimerStrategy" />.
	/// </summary>
	/// <param name="mode">The timer mode.</param>
	public TimerStrategy(TimerMode mode)
	{
		Mode = mode;
	}
}
