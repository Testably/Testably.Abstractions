#if FEATURE_PERIODIC_TIMER
using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Testing.TimeSystem;

/// <summary>
///     Additional abstractions for a mocked periodic timer to simplify testing.<br />
///     Implements <see cref="IPeriodicTimer" />.
/// </summary>
public interface IPeriodicTimerMock : IPeriodicTimer
{
	/// <summary>
	///     Gets the total number of times <see cref="IPeriodicTimer.WaitForNextTickAsync" /> has been entered
	///     since the periodic timer was created.<br />
	///     The counter is not affected by changes to <see cref="IPeriodicTimer.Period" /> and stops increasing
	///     once the periodic timer is disposed.
	/// </summary>
	long WaitForNextTickCount { get; }
}
#endif
