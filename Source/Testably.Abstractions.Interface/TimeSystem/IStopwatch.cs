using System;
using System.Diagnostics;

namespace Testably.Abstractions.TimeSystem;

/// <summary>
///     Abstractions for <see cref="Stopwatch" />.
/// </summary>
public interface IStopwatch : ITimeSystemEntity
{
	/// <inheritdoc cref="Stopwatch.Elapsed" />
	TimeSpan Elapsed { get; }

	/// <inheritdoc cref="Stopwatch.ElapsedMilliseconds" />
	long ElapsedMilliseconds { get; }

	/// <inheritdoc cref="Stopwatch.ElapsedTicks" />
	long ElapsedTicks { get; }

	/// <inheritdoc cref="Stopwatch.IsRunning" />
	bool IsRunning { get; }

	/// <inheritdoc cref="Stopwatch.Reset()" />
	void Reset();

	/// <inheritdoc cref="Stopwatch.Restart()" />
	void Restart();

	/// <inheritdoc cref="Stopwatch.Start()" />
	void Start();

	/// <inheritdoc cref="Stopwatch.Stop()" />
	void Stop();
}
