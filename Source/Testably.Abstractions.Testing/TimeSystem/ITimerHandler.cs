namespace Testably.Abstractions.Testing.TimeSystem;

/// <summary>
///     The timer handler gives access to all registered timers.
/// </summary>
public interface ITimerHandler
{
	/// <summary>
	///     Index over all registered timers.
	/// </summary>
	ITimerMock this[int index] { get; }
}
