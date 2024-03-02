using System;
using System.Collections.Generic;
using System.Threading;

namespace Testably.Abstractions.Testing.Statistics;

internal class CallStatistics : IStatistics
{
	private readonly List<CallStatistic> _calls = new();
	public IReadOnlyList<CallStatistic> Calls => _calls.AsReadOnly();
	public static CallStatistics Empty { get; } = new();

	private static readonly AsyncLocal<bool> IsDisabled = new();

	internal IDisposable Register(string name, params object?[] parameters)
	{
		if (IsDisabled.Value)
		{
			return TemporaryDisable.None;
		}
		IsDisabled.Value = true;
		_calls.Add(new CallStatistic(name, parameters));
		return new TemporaryDisable(() => IsDisabled.Value = false);
	}

	private class TemporaryDisable : IDisposable
	{
		public static IDisposable None { get; } = new TemporaryDisable(() => { });

		private readonly Action _onDispose;

		public TemporaryDisable(Action onDispose)
		{
			_onDispose = onDispose;
		}

		/// <inheritdoc />
		public void Dispose() => _onDispose();
	}
}
