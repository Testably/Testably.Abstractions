﻿using System;
#if NETSTANDARD2_0
using Testably.Abstractions.TimeSystem;
#endif

namespace Testably.Abstractions.Testing.TimeSystem;

internal sealed class TimeProviderMock : ITimeProvider
{
	private DateTime _now;
	private readonly string _description;
#if NET9_0_OR_GREATER
	private readonly System.Threading.Lock _lock = new();
#else
	private readonly object _lock = new();
#endif

	public TimeProviderMock(DateTime now, string description)
	{
		_now = now;
		_description = description;
	}

	#region ITimeProvider Members

	/// <inheritdoc cref="ITimeProvider.MaxValue" />
	public DateTime MaxValue { get; set; } = DateTime.MaxValue;

	/// <inheritdoc cref="ITimeProvider.MinValue" />
	public DateTime MinValue { get; set; } = DateTime.MinValue;

#if NETSTANDARD2_0
	/// <inheritdoc cref="IDateTime.UnixEpoch" />
	public DateTime UnixEpoch { get; set; } =
		new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
#else
	/// <inheritdoc cref="ITimeProvider.UnixEpoch" />
	public DateTime UnixEpoch { get; set; } = DateTime.UnixEpoch;
#endif

	/// <inheritdoc cref="ITimeProvider.AdvanceBy(TimeSpan)" />
	public void AdvanceBy(TimeSpan interval)
	{
		lock (_lock)
		{
			_now = _now.Add(interval);
		}
	}

	/// <inheritdoc cref="ITimeProvider.Read()" />
	public DateTime Read()
	{
		return _now;
	}

	/// <inheritdoc cref="ITimeProvider.SetTo(DateTime)" />
	public void SetTo(DateTime value)
	{
		lock (_lock)
		{
			_now = value;
		}
	}

	#endregion

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> _description;
}
