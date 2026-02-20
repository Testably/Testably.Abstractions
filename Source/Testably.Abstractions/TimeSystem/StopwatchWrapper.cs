using System;
using System.Diagnostics;

namespace Testably.Abstractions.TimeSystem;

internal sealed class StopwatchWrapper : IStopwatch
{
	private readonly Stopwatch _stopwatch;

	internal StopwatchWrapper(ITimeSystem timeSystem, Stopwatch stopwatch)
	{
		TimeSystem = timeSystem;
		_stopwatch = stopwatch;
	}

	#region IStopwatch Members

	/// <inheritdoc cref="IStopwatch.Elapsed" />
	public TimeSpan Elapsed
		=> _stopwatch.Elapsed;

	/// <inheritdoc cref="IStopwatch.ElapsedMilliseconds" />
	public long ElapsedMilliseconds
		=> _stopwatch.ElapsedMilliseconds;

	/// <inheritdoc cref="IStopwatch.ElapsedTicks" />
	public long ElapsedTicks
		=> _stopwatch.ElapsedTicks;

	/// <inheritdoc cref="IStopwatch.IsRunning" />
	public bool IsRunning
		=> _stopwatch.IsRunning;

	/// <inheritdoc cref="ITimeSystemEntity.TimeSystem" />
	public ITimeSystem TimeSystem { get; }

	/// <inheritdoc cref="IStopwatch.Reset()" />
	public void Reset()
		=> _stopwatch.Reset();

	/// <inheritdoc cref="IStopwatch.Restart()" />
	public void Restart()
		=> _stopwatch.Restart();

	/// <inheritdoc cref="IStopwatch.Start()" />
	public void Start()
		=> _stopwatch.Start();

	/// <inheritdoc cref="IStopwatch.Stop()" />
	public void Stop()
		=> _stopwatch.Stop();

	#endregion
}
