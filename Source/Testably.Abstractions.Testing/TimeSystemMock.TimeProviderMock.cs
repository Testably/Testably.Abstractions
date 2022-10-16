﻿using System;
using System.Threading;

namespace Testably.Abstractions.Testing;

public sealed partial class TimeSystemMock
{
	internal sealed class TimeProviderMock : ITimeProvider
	{
		private static readonly AsyncLocal<DateTime> Now = new();

		public TimeProviderMock(DateTime now)
		{
			Now.Value = now;
		}

		#region ITimeProvider Members

		/// <inheritdoc cref="TimeSystemMock.ITimeProvider.MaxValue" />
		public DateTime MaxValue { get; set; } = System.DateTime.MaxValue;

		/// <inheritdoc cref="TimeSystemMock.ITimeProvider.MinValue" />
		public DateTime MinValue { get; set; } = System.DateTime.MinValue;

#if NETSTANDARD2_0
		/// <inheritdoc cref="ITimeSystem.IDateTime.UnixEpoch" />
		public DateTime UnixEpoch { get; set; } =
			new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
#else
		/// <inheritdoc cref="TimeSystemMock.ITimeProvider.UnixEpoch" />
		public DateTime UnixEpoch { get; set; } = System.DateTime.UnixEpoch;
#endif

		/// <inheritdoc cref="TimeSystemMock.ITimeProvider.AdvanceBy(TimeSpan)" />
		public void AdvanceBy(TimeSpan interval)
		{
			Now.Value = Now.Value.Add(interval);
		}

		/// <inheritdoc cref="TimeSystemMock.ITimeProvider.Read()" />
		public DateTime Read()
		{
			return Now.Value;
		}

		/// <inheritdoc cref="TimeSystemMock.ITimeProvider.SetTo(DateTime)" />
		public void SetTo(DateTime value)
		{
			Now.Value = value;
		}

		#endregion
	}
}