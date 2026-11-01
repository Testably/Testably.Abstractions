using System.Threading;

namespace Testably.Abstractions.Testing.TimeSystem;

/// <summary>
///     The strategy how to handle mocked timers for testing.
/// </summary>
public sealed class TimerStrategy
{
	/// <summary>
	///     The default timer strategy uses <see cref="TimerMode.StartImmediately" /> and
	///     does not swallow exceptions.
	/// </summary>
	public static TimerStrategy Default { get; }
		= new();

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

	/// <summary>
	///     The timer mode.
	/// </summary>
	public TimerMode Mode { get; }

	/// <summary>
	///     Flag indicating, if exceptions in the <see cref="TimerCallback" /> should be swallowed or thrown.
	///     <para />
	///     The real <see cref="Timer" /> will crash the application in case of an exception in the
	///     <see cref="TimerCallback" />.
	/// </summary>
	public bool SwallowExceptions { get; }
}
