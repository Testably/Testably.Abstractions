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
}
