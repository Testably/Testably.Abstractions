namespace Testably.Abstractions.Testing.TimeSystem;

/// <summary>
///     The timer handler gives access to all registered timers.
/// </summary>
public interface ITimerHandler
{
	/// <summary>
	///     Gets the timer associated with the specified <paramref name="index" />.
	/// </summary>
	/// <param name="index">A zero-based incremented integer according to the order of the timer registration.</param>
	ITimerMock this[int index] { get; }
}
