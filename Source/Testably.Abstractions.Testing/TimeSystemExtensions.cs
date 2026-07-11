#if FEATURE_TIMEPROVIDER
using System;
using System.Threading;
using System.Threading.Tasks;
using ITimer = Testably.Abstractions.TimeSystem.ITimer;

namespace Testably.Abstractions.Testing;

/// <summary>
///     Extension methods for <see cref="ITimeSystem" />.
/// </summary>
public static class TimeSystemExtensions
{
	/// <summary>
	///     Wraps the <paramref name="timeSystem" /> as a <see cref="System.TimeProvider" />, so that it can be injected into
	///     code that consumes the built-in .NET time abstraction (e.g. the <see cref="System.Threading.PeriodicTimer" />
	///     or <see cref="System.Threading.CancellationTokenSource" /> constructors that accept a
	///     <see cref="System.TimeProvider" />, the <c>Task.Delay</c> / <c>Task.WaitAsync</c> overloads, or
	///     <c>Microsoft.Extensions.*</c>).
	///     <para />
	///     When called on a <see cref="MockTimeSystem" />, the returned <see cref="System.TimeProvider" /> is fully
	///     controllable: advancing the mock clock (e.g. via <see cref="TimeSystem.ITimeProvider.AdvanceBy(TimeSpan)" />) is observable
	///     through <see cref="System.TimeProvider.GetUtcNow()" /> and <see cref="System.TimeProvider.GetTimestamp()" />.
	///     <para />
	///     Timers created through <see cref="System.TimeProvider.CreateTimer(TimerCallback, object, TimeSpan, TimeSpan)" />
	///     keep firing even when the callback throws (the exception is swallowed), matching a real
	///     <see cref="Timer" /> regardless of the <see cref="TimeSystem.ITimerStrategy" /> configured on a
	///     <see cref="MockTimeSystem" />.
	/// </summary>
	public static System.TimeProvider ToTimeProvider(this ITimeSystem timeSystem)
	{
		ArgumentNullException.ThrowIfNull(timeSystem);
		return new TimeSystemTimeProvider(timeSystem);
	}

	private sealed class TimeSystemTimeProvider(ITimeSystem timeSystem) : System.TimeProvider
	{
		/// <inheritdoc cref="System.TimeProvider.LocalTimeZone" />
		public override TimeZoneInfo LocalTimeZone
			=> timeSystem.TimeZoneInfo.Local;

		/// <inheritdoc cref="System.TimeProvider.TimestampFrequency" />
		public override long TimestampFrequency
			=> timeSystem.Stopwatch.Frequency;

		/// <inheritdoc cref="System.TimeProvider.CreateTimer(TimerCallback, object, TimeSpan, TimeSpan)" />
		public override System.Threading.ITimer CreateTimer(
			TimerCallback callback,
			object? state,
			TimeSpan dueTime,
			TimeSpan period)
			=> new TimerAdapter(
				timeSystem.Timer.New(WrapCallback(callback), state, dueTime, period));

		/// <inheritdoc cref="System.TimeProvider.GetTimestamp()" />
		public override long GetTimestamp()
			=> timeSystem.Stopwatch.GetTimestamp();

		/// <inheritdoc cref="System.TimeProvider.GetUtcNow()" />
		public override DateTimeOffset GetUtcNow()
			=> new(timeSystem.DateTime.UtcNow);

		/// <summary>
		///     Wraps the <paramref name="callback" /> so that an exception thrown by it is swallowed and does not stop the
		///     timer. This mirrors a real <see cref="Timer" />, whose schedule keeps firing on subsequent periods regardless
		///     of a callback exception, and makes the bridged timer behave the same independently of the
		///     <see cref="TimeSystem.ITimerStrategy" /> configured on a <see cref="MockTimeSystem" />.
		/// </summary>
		private static TimerCallback WrapCallback(TimerCallback callback)
			=> state =>
			{
				try
				{
					callback(state);
				}
				catch (Exception)
				{
					// Swallow so the timer keeps firing, matching a real timer whose schedule
					// is not cancelled by a throwing callback (see ITimerStrategy).
				}
			};
	}

	/// <summary>
	///     Adapts the <see cref="ITimer" /> of the <see cref="ITimeSystem" /> to the
	///     <see cref="System.Threading.ITimer" /> expected by <see cref="System.TimeProvider" />.
	/// </summary>
	private sealed class TimerAdapter(ITimer timer) : System.Threading.ITimer
	{
		#region ITimer Members

		/// <inheritdoc cref="System.Threading.ITimer.Change(TimeSpan, TimeSpan)" />
		public bool Change(TimeSpan dueTime, TimeSpan period)
			=> timer.Change(dueTime, period);

		/// <inheritdoc cref="IDisposable.Dispose()" />
		public void Dispose()
			=> timer.Dispose();

		/// <inheritdoc cref="IAsyncDisposable.DisposeAsync()" />
		public ValueTask DisposeAsync()
			=> timer.DisposeAsync();

		#endregion
	}
}
#endif
