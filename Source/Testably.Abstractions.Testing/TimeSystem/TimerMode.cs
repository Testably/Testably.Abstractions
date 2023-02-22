namespace Testably.Abstractions.Testing.TimeSystem;

/// <summary>
///     The timer mode.
/// </summary>
public enum TimerMode
{
	/// <summary>
	///     Start the timer thread immediately.
	///     <para />
	///     This is the normal behaviour of real <see cref="System.Threading.Timer" />s.
	/// </summary>
	StartImmediately = 1,

	/// <summary>
	///     Wait execution of timers until a <see cref="ITimerMock.Wait(int, int, System.Action{ITimerMock})" /> is called.
	///     <para />
	///     This simplifies certain test cases.
	/// </summary>
	StartOnMockWait = 2,
}
