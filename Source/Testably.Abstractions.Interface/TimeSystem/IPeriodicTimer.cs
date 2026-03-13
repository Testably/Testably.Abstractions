#if FEATURE_PERIODIC_TIMER
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.TimeSystem;

/// <summary>
///     Abstractions for <see cref="PeriodicTimer" />.
/// </summary>
public interface IPeriodicTimer : ITimeSystemEntity, IDisposable
{
	/// <inheritdoc cref="PeriodicTimer.Period" />
	TimeSpan Period { get; set; }

	/// <inheritdoc cref="PeriodicTimer.WaitForNextTickAsync(CancellationToken)" />
	ValueTask<bool> WaitForNextTickAsync(CancellationToken cancellationToken = default);
}
#endif
