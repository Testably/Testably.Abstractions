using System;
using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Testing.TimeSystem;

internal sealed class StopwatchMock : IStopwatch
{
	private long _elapsedTicks;
	private readonly MockTimeSystem _mockTimeSystem;
	private DateTime? _start;
	private readonly long _tickPeriod;

	internal StopwatchMock(MockTimeSystem timeSystem, long tickPeriod)
	{
		_mockTimeSystem = timeSystem;
		_tickPeriod = tickPeriod;
	}

	#region IStopwatch Members

	/// <inheritdoc cref="IStopwatch.Elapsed" />
	public TimeSpan Elapsed
		=> TimeSpan.FromTicks(ElapsedTicks / _tickPeriod);

	/// <inheritdoc cref="IStopwatch.ElapsedMilliseconds" />
	public long ElapsedMilliseconds
		=> ElapsedTicks / _tickPeriod / TimeSpan.TicksPerMillisecond;

	/// <inheritdoc cref="IStopwatch.ElapsedTicks" />
	public long ElapsedTicks
	{
		get
		{
			long timeElapsed = _elapsedTicks;

			// If the Stopwatch is running, add elapsed time since the Stopwatch is started last time.
			if (_start is not null)
			{
				timeElapsed += (_mockTimeSystem.TimeProvider.Read() - _start.Value).Ticks
				               * _tickPeriod;
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
		if (_start is null)
		{
			_start = _mockTimeSystem.TimeProvider.Read();
		}
	}

	/// <inheritdoc cref="IStopwatch.Stop()" />
	public void Stop()
	{
		if (_start.HasValue)
		{
			_elapsedTicks += (_mockTimeSystem.TimeProvider.Read() - _start.Value).Ticks *
			                 _tickPeriod;
			_start = null;
		}
	}

	#endregion
}
