using System.Threading;

namespace Testably.Abstractions.Testing.TimeSystem;

/// <summary>
///     The strategy how to handle mocked timers for testing.
/// </summary>
public interface ITimerStrategy
{
	/// <summary>
	///     The timer mode.
	/// </summary>
	TimerMode Mode { get; }

	/// <summary>
	///     Flag indicating, if exceptions in the <see cref="TimerCallback" /> should be swallowed or thrown.
	///     <para />
	///     The real <see cref="Timer" /> will crash the application in case of an exception in the
	///     <see cref="TimerCallback" />.
	/// </summary>
	bool SwallowExceptions { get; }
}
