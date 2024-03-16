namespace Testably.Abstractions.Testing.TimeSystem;

/// <summary>
///     The timer strategy.
/// </summary>
public class TimerStrategy : ITimerStrategy
{
	/// <summary>
	///     The default timer strategy uses <see cref="TimerMode.StartImmediately" /> and
	///     does not swallow exceptions.
	/// </summary>
	public static ITimerStrategy Default { get; }
		= new TimerStrategy();

	/// <summary>
	///     Initializes a new instance of <see cref="TimerStrategy" />.
	/// </summary>
	/// <param name="mode">The timer mode.</param>
	/// <param name="swallowExceptions">Flag, indicating if exceptions should be swallowed.</param>
	public TimerStrategy(
		TimerMode mode = TimerMode.StartImmediately,
		bool swallowExceptions = false)
	{
		Mode = mode;
		SwallowExceptions = swallowExceptions;
	}

	#region ITimerStrategy Members

	/// <inheritdoc cref="ITimerStrategy.Mode" />
	public TimerMode Mode { get; }

	/// <inheritdoc cref="ITimerStrategy.SwallowExceptions" />
	public bool SwallowExceptions { get; }

	#endregion
}
