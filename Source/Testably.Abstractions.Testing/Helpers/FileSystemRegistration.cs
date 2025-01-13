using System;
using System.Threading;
using Testably.Abstractions.Testing.Statistics;

namespace Testably.Abstractions.Testing.Helpers;

internal sealed class FileSystemRegistration : IStatisticsGate
{
	private static readonly AsyncLocal<bool> IsDisabled = new();
	private static readonly AsyncLocal<bool> IsInit = new();

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
		if (IsDisabled.Value)
		{
			release = TemporaryDisable.None;
			return false;
		}

		IsDisabled.Value = true;
		release = new TemporaryDisable(() => IsDisabled.Value = false);
		return true;
	}

	#endregion

	/// <summary>
	///     Ignores all registrations until the return value is disposed.
	/// </summary>
	internal static IDisposable Ignore()
	{
		if (IsDisabled.Value)
		{
			return TemporaryDisable.None;
		}

		IsDisabled.Value = true;
		IsInit.Value = true;
		return new TemporaryDisable(() =>
		{
			IsDisabled.Value = false;
			IsInit.Value = false;
		});
	}

	internal static bool IsInitializing()
		=> IsInit.Value;

	private sealed class TemporaryDisable : IDisposable
	{
		public static IDisposable None { get; } = new NoOpDisposable();

		private readonly Action _onDispose;

		public TemporaryDisable(Action onDispose)
		{
			_onDispose = onDispose;
		}

		#region IDisposable Members

		/// <inheritdoc cref="IDisposable.Dispose()" />
		public void Dispose() => _onDispose();

		#endregion
	}
}
