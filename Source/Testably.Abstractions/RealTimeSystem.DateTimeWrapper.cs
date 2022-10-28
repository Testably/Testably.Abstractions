using System;

namespace Testably.Abstractions;

public sealed partial class RealTimeSystem
{
	private sealed class DateTimeWrapper : IDateTime
	{
		internal DateTimeWrapper(RealTimeSystem timeSystem)
		{
			TimeSystem = timeSystem;
		}

		#region IDateTime Members

		/// <inheritdoc cref="IDateTime.MaxValue" />
		public DateTime MaxValue
			=> System.DateTime.MaxValue;

		/// <inheritdoc cref="IDateTime.MinValue" />
		public DateTime MinValue
			=> System.DateTime.MinValue;

#if NETSTANDARD2_0
		/// <inheritdoc cref="IDateTime.UnixEpoch" />
		public DateTime UnixEpoch => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
#else
		/// <inheritdoc cref="IDateTime.UnixEpoch" />
		public DateTime UnixEpoch
			=> System.DateTime.UnixEpoch;
#endif

		/// <inheritdoc cref="IDateTime.Now" />
		public DateTime Now
			=> System.DateTime.Now;

		/// <inheritdoc cref="IDateTime.UtcNow" />
		public DateTime UtcNow
			=> System.DateTime.UtcNow;

		/// <inheritdoc cref="IDateTime.Today" />
		public DateTime Today
			=> System.DateTime.Today;

		/// <inheritdoc cref="ITimeSystemExtensionPoint.TimeSystem" />
		public ITimeSystem TimeSystem { get; }

		#endregion
	}
}