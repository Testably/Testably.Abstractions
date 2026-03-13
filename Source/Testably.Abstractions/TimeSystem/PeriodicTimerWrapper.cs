#if FEATURE_PERIODIC_TIMER
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.TimeSystem;

internal sealed class PeriodicTimerWrapper : IPeriodicTimer
{
	private readonly PeriodicTimer _periodicTimer;

	internal PeriodicTimerWrapper(ITimeSystem timeSystem, PeriodicTimer periodicTimer)
	{
		TimeSystem = timeSystem;
		_periodicTimer = periodicTimer;
	}

	#region IPeriodicTimer Members

	/// <inheritdoc cref="IPeriodicTimer.Period" />
	public TimeSpan Period
	{
		get => _periodicTimer.Period;
		set => _periodicTimer.Period = value;
	}

	/// <inheritdoc cref="ITimeSystemEntity.TimeSystem" />
	public ITimeSystem TimeSystem { get; }

	/// <inheritdoc cref="IDisposable.Dispose()" />
	public void Dispose()
		=> _periodicTimer.Dispose();

	/// <inheritdoc cref="IPeriodicTimer.WaitForNextTickAsync(CancellationToken)" />
	public ValueTask<bool> WaitForNextTickAsync(CancellationToken cancellationToken = default)
		=> _periodicTimer.WaitForNextTickAsync(cancellationToken);

	#endregion
}
#endif
