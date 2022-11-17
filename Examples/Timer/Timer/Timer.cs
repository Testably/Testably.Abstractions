using System;
using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.Examples.Timer;

/// <summary>
///     A timer.
/// </summary>
public sealed class Timer : IDisposable
{
	/// <summary>
	///     The action to execute in each iteration.
	/// </summary>
	public Action<CancellationToken> Callback { get; }

	/// <summary>
	///     The interval in which to execute the <see cref="Callback" />.
	/// </summary>
	public TimeSpan Interval { get; }

	/// <summary>
	///     Flag indicating, if the timer is running or not.
	/// </summary>
	public bool IsRunning { get; private set; }

	/// <summary>
	///     (optional) A callback for handling errors thrown by the <see cref="Callback" />.
	/// </summary>
	public Action<Exception>? OnError { get; }

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

	#region IDisposable Members

	/// <inheritdoc cref="IDisposable.Dispose()" />
	public void Dispose()
	{
		Stop();
		_runningCancellationTokenSource?.Dispose();
		_linkedCancellationTokenSource?.Dispose();
	}

	#endregion

	/// <summary>
	///     Starts the timer.
	/// </summary>
	public Timer Start(CancellationToken cancellationToken = default)
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

	/// <summary>
	///     Stops the current timer.
	/// </summary>
	public Timer Stop()
	{
		IsRunning = false;
		if (_runningCancellationTokenSource is { IsCancellationRequested: false })
		{
			_runningCancellationTokenSource?.Cancel();
		}

		return this;
	}
}
