using System;
using System.Collections.Generic;

namespace Testably.Abstractions.Testing.Statistics;

internal sealed class MockStatistics : IStatistics
{
	private readonly List<CallStatistic> _calls = new();
	public IReadOnlyList<CallStatistic> Calls => _calls.AsReadOnly();

	private bool _isDisabled;

	internal IDisposable Register(string name, params object?[] parameters)
	{
		if (_isDisabled)
		{
			return TemporaryDisable.None;
		}
		_isDisabled = true;
		_calls.Add(new CallStatistic(name, parameters));
		return new TemporaryDisable(() => _isDisabled = false);
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
