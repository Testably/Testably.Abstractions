using System;
using System.Threading;
#if FEATURE_ASYNC_DISPOSABLE
using System.Threading.Tasks;
#endif

namespace Testably.Abstractions.TimeSystem;

internal sealed class TimerWrapper : ITimer
{
	private readonly Timer _timer;

	internal TimerWrapper(ITimeSystem timeSystem, Timer timer)
	{
		TimeSystem = timeSystem;
		_timer = timer;
	}

	#region ITimer Members

	/// <inheritdoc cref="ITimeSystemEntity.TimeSystem" />
	public ITimeSystem TimeSystem { get; }

	/// <inheritdoc cref="ITimer.Change(int, int)" />
	public bool Change(int dueTime, int period)
		=> _timer.Change(dueTime, period);

	/// <inheritdoc cref="ITimer.Change(long, long)" />
	public bool Change(long dueTime, long period)
		=> _timer.Change(dueTime, period);

	/// <inheritdoc cref="ITimer.Change(TimeSpan, TimeSpan)" />
	public bool Change(TimeSpan dueTime, TimeSpan period)
		=> _timer.Change(dueTime, period);

	/// <inheritdoc cref="IDisposable.Dispose()" />
	public void Dispose()
		=> _timer.Dispose();

	/// <inheritdoc cref="ITimer.Dispose(WaitHandle)" />
	public bool Dispose(WaitHandle notifyObject)
		=> _timer.Dispose(notifyObject);

#if FEATURE_ASYNC_DISPOSABLE
	/// <inheritdoc cref="IAsyncDisposable.DisposeAsync()" />
	public ValueTask DisposeAsync()
		=> _timer.DisposeAsync();
#endif

	#endregion
}
