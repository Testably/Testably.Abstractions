using System;
using System.Threading;
using Testably.Abstractions.Testing.Statistics;

namespace Testably.Abstractions.Testing.Helpers;

internal sealed class FileSystemRegistration : IStatisticsGate
{
	/// <summary>
	///     <see langword="null" /> while registrations are enabled. Shared sentinels instead of a <see langword="bool" />
	///     avoid boxing, and resetting to <see langword="null" /> removes the entry from the execution context.
	/// </summary>
	private static readonly AsyncLocal<object?> State = new();

	private static readonly object Disabled = new();
	private static readonly object Initializing = new();

	/// <summary>
	///     The total count of registered statistic calls.
	/// </summary>
	public int TotalCount => _counter;

	private int _counter;

	#region IStatisticsGate Members

	/// <inheritdoc cref="IStatisticsGate.GetCounter()" />
	public int GetCounter()
	{
		return Interlocked.Increment(ref _counter);
	}

	/// <inheritdoc cref="IStatisticsGate.TryGetLock(out IDisposable)" />
	public bool TryGetLock(out IDisposable release)
	{
		if (State.Value != null)
		{
			release = TemporaryDisable.None;
			return false;
		}

		State.Value = Disabled;
		release = TemporaryDisable.Instance;
		return true;
	}

	#endregion

	/// <summary>
	///     Ignores all registrations until the return value is disposed.
	/// </summary>
	internal static IDisposable Ignore()
	{
		if (State.Value != null)
		{
			return TemporaryDisable.None;
		}

		State.Value = Initializing;
		return TemporaryDisable.Instance;
	}

	internal static bool IsInitializing()
		=> ReferenceEquals(State.Value, Initializing);

	private sealed class TemporaryDisable : IDisposable
	{
		public static IDisposable Instance { get; } = new TemporaryDisable();
		public static IDisposable None { get; } = new NoOpDisposable();

		#region IDisposable Members

		/// <inheritdoc cref="IDisposable.Dispose()" />
		public void Dispose() => State.Value = null;

		#endregion
	}
}
