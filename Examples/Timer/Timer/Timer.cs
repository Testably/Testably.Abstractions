using System;
using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.Examples.Timer;

internal sealed class Timer : ITimer
{
	private CancellationTokenSource? _linkedCancellationTokenSource;
	private CancellationTokenSource? _runningCancellationTokenSource;
	private readonly ITimeSystem _timeSystem;

	public Timer(
		ITimeSystem timeSystem,
		TimeSpan interval,
		Action<CancellationToken> callback,
		Action<Exception>? onError)
	{
		_timeSystem = timeSystem;
		Interval = interval;
		Callback = callback;
		OnError = onError;
	}

	#region ITimer Members

	/// <inheritdoc cref="ITimer.Callback" />
	public Action<CancellationToken> Callback { get; }

	/// <inheritdoc cref="ITimer.Interval" />
	public TimeSpan Interval { get; }

	/// <inheritdoc cref="ITimer.IsRunning" />
	public bool IsRunning { get; private set; }

	/// <inheritdoc cref="ITimer.OnError" />
	public Action<Exception>? OnError { get; }

	/// <inheritdoc cref="IDisposable.Dispose()" />
	public void Dispose()
	{
		Stop();
		_runningCancellationTokenSource?.Dispose();
		_linkedCancellationTokenSource?.Dispose();
	}

	/// <inheritdoc cref="ITimer.Start(CancellationToken)" />
	public ITimer Start(CancellationToken cancellationToken = default)
	{
		Stop();
		IsRunning = true;
		CancellationTokenSource runningCancellationTokenSource =
			_runningCancellationTokenSource = new CancellationTokenSource();
		CancellationToken internalToken = _runningCancellationTokenSource.Token;
		CancellationTokenSource linkedCts = _linkedCancellationTokenSource =
			CancellationTokenSource.CreateLinkedTokenSource(
				internalToken, cancellationToken);
		CancellationToken token = linkedCts.Token;
		Task.Factory.StartNew(
				() =>
				{
					DateTime nextPlannedExecution = _timeSystem.DateTime.UtcNow;
					while (!token.IsCancellationRequested)
					{
						nextPlannedExecution += Interval;
						try
						{
							Callback(token);
						}
						catch (Exception e)
						{
							OnError?.Invoke(e);
						}

						TimeSpan delay = nextPlannedExecution - _timeSystem.DateTime.UtcNow;
						if (delay > TimeSpan.Zero)
						{
							_timeSystem.Task.TryDelay(delay, token);
						}
					}
				}, cancellationToken: token,
				TaskCreationOptions.LongRunning, TaskScheduler.Default)
			.ContinueWith(_ =>
			{
				runningCancellationTokenSource.Dispose();
				linkedCts.Dispose();
			}, TaskScheduler.Default);
		return this;
	}

	/// <inheritdoc cref="ITimer.Stop()" />
	public ITimer Stop()
	{
		IsRunning = false;
		if (_runningCancellationTokenSource is { IsCancellationRequested: false })
		{
			_runningCancellationTokenSource?.Cancel();
		}

		return this;
	}

	#endregion
}
