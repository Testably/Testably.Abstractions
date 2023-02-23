using System;
using System.Threading;

namespace Testably.Abstractions.TimeSystem;

/// <summary>
///     Abstractions for <see cref="Timer" />.
/// </summary>
public interface ITimer : ITimeSystemEntity, IDisposable
#if FEATURE_ASYNC_DISPOSABLE
, IAsyncDisposable
#endif
{
	/// <inheritdoc cref="Timer.Change(int, int)" />
	bool Change(int dueTime, int period);

	/// <inheritdoc cref="Timer.Change(long, long)" />
	bool Change(long dueTime, long period);

	/// <inheritdoc cref="Timer.Change(TimeSpan, TimeSpan)" />
	bool Change(TimeSpan dueTime, TimeSpan period);

	/// <inheritdoc cref="Timer.Dispose(WaitHandle)" />
	bool Dispose(WaitHandle notifyObject);
}
