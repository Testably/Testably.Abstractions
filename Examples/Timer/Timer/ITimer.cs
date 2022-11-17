using System;
using System.Threading;

namespace Timer;

/// <summary>
///     A timer.
/// </summary>
public interface ITimer : IDisposable
{
	/// <summary>
	///     The action to execute in each iteration.
	/// </summary>
	Action<CancellationToken> Callback { get; }

	/// <summary>
	///     The interval in which to execute the <see cref="Callback" />.
	/// </summary>
	TimeSpan Interval { get; }

	/// <summary>
	///     Flag indicating, if the timer is running or not.
	/// </summary>
	bool IsRunning { get; }

	/// <summary>
	///     (optional) A callback for handling errors thrown by the <see cref="Callback" />.
	/// </summary>
	Action<Exception>? OnError { get; }

	/// <summary>
	///     Starts the timer.
	/// </summary>
	ITimer Start(CancellationToken cancellationToken = default);

	/// <summary>
	///     Stops the current timer.
	/// </summary>
	ITimer Stop();
}
