using System;
using System.Threading;
using Testably.Abstractions;

namespace TimerExample;

public sealed class SynchronizationTimer : IDisposable
{
	private readonly Action<Exception>? _errorHandler;
	private readonly Action<int> _executeCallback;
	private readonly TimeSpan _interval;
	private int _iterationCount;
	private IRunningTimer? _runningTimer;
	private readonly ITimeSystem _timeSystem;

	public SynchronizationTimer(
		ITimeSystem timeSystem,
		TimeSpan interval,
		Action<int> executeCallback,
		Action<Exception>? errorHandler = null)
	{
		_timeSystem = timeSystem;
		_interval = interval;
		_executeCallback = executeCallback;
		_errorHandler = errorHandler;
	}

	#region IDisposable Members

	/// <inheritdoc />
	public void Dispose()
	{
		_runningTimer?.Dispose();
	}

	#endregion

	public void Start(CancellationToken cancellationToken)
	{
		_runningTimer = _timeSystem.CreateTimer(
				_interval,
				_ => _executeCallback.Invoke(++_iterationCount),
				_errorHandler)
			.Start(cancellationToken);
	}
}