using System;
using System.Threading;
using Testably.Abstractions.Testing.TimeSystem;

namespace Testably.Abstractions.Examples.ThreadAwareTimeProvider;

public sealed class ThreadAwareTimeProvider : ITimeProvider
{
	private static readonly AsyncLocal<DateTime> Now = new();
	private static readonly AsyncLocal<DateTime?> SynchronizedTime = new();
	private DateTime? _synchronizedTime;

	public ThreadAwareTimeProvider(DateTime? now = null)
	{
		Now.Value = now ?? DateTime.Now;
	}

	#region ITimeProvider Members

	/// <inheritdoc cref="ITimeProvider.MaxValue" />
	public DateTime MaxValue { get; set; } = DateTime.MaxValue;

	/// <inheritdoc cref="ITimeProvider.MinValue" />
	public DateTime MinValue { get; set; } = DateTime.MinValue;

	/// <inheritdoc cref="ITimeProvider.UnixEpoch" />
	public DateTime UnixEpoch { get; set; } = DateTime.UnixEpoch;

	/// <inheritdoc cref="ITimeProvider.AdvanceBy(TimeSpan)" />
	public void AdvanceBy(TimeSpan interval)
	{
		CheckSynchronization();
		Now.Value = Now.Value.Add(interval);
	}

	/// <inheritdoc cref="ITimeProvider.Read()" />
	public DateTime Read()
	{
		CheckSynchronization();
		return Now.Value;
	}

	/// <inheritdoc cref="ITimeProvider.SetTo(DateTime)" />
	public void SetTo(DateTime value)
	{
		Now.Value = value;
	}

	#endregion

	/// <summary>
	///     Synchronizes the currently simulated system time across all async contexts.
	/// </summary>
	/// <remarks>
	///     This means that in multi-thread or async environments after this call all clocks start with the value from the
	///     calling async context.<br />
	///     (see also <see href="https://learn.microsoft.com/en-us/dotnet/api/system.threading.asynclocal-1" />)
	/// </remarks>
	public void SynchronizeClock()
	{
		_synchronizedTime = Now.Value;
	}

	private void CheckSynchronization()
	{
		if (_synchronizedTime != null &&
		    SynchronizedTime.Value != _synchronizedTime)
		{
			SynchronizedTime.Value = _synchronizedTime;
			Now.Value = _synchronizedTime.Value;
		}
	}
}
