using System;
using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Testing.TimeSystem;

internal sealed class StopwatchMock : IStopwatch
{
	private long _elapsedTicks;
	private readonly MockTimeSystem _mockTimeSystem;
	private DateTime? _start;

	internal StopwatchMock(MockTimeSystem timeSystem)
	{
		_mockTimeSystem = timeSystem;
	}

	#region IStopwatch Members

	/// <inheritdoc cref="IStopwatch.Elapsed" />
	public TimeSpan Elapsed
		=> new(ElapsedTicks);

	/// <inheritdoc cref="IStopwatch.ElapsedMilliseconds" />
	public long ElapsedMilliseconds
		=> ElapsedTicks / TimeSpan.TicksPerMillisecond;

	/// <inheritdoc cref="IStopwatch.ElapsedTicks" />
	public long ElapsedTicks
	{
		get
		{
			long timeElapsed = _elapsedTicks;

			// If the Stopwatch is running, add elapsed time since the Stopwatch is started last time.
			if (_start is not null)
			{
				timeElapsed += (_mockTimeSystem.TimeProvider.Read() - _start.Value).Ticks;
			}

			return timeElapsed;
		}
	}

	/// <inheritdoc cref="IStopwatch.IsRunning" />
	public bool IsRunning => _start is not null;

	/// <inheritdoc cref="ITimeSystemEntity.TimeSystem" />
	public ITimeSystem TimeSystem
		=> _mockTimeSystem;

	/// <inheritdoc cref="IStopwatch.Reset()" />
	public void Reset()
	{
		Stop();
		_elapsedTicks = 0;
	}

	/// <inheritdoc cref="IStopwatch.Restart()" />
	public void Restart()
	{
		Reset();
		Start();
	}

	/// <inheritdoc cref="IStopwatch.Start()" />
	public void Start()
	{
		_start = _mockTimeSystem.TimeProvider.Read();
	}

	/// <inheritdoc cref="IStopwatch.Stop()" />
	public void Stop()
	{
		if (_start.HasValue)
		{
			_elapsedTicks += (_mockTimeSystem.TimeProvider.Read() - _start.Value).Ticks;
			_start = null;
		}
	}

	#endregion
}
