using System;

namespace Testably.Abstractions;

public sealed partial class RealTimeSystem
{
	private sealed class ThreadWrapper : IThread
	{
		internal ThreadWrapper(RealTimeSystem timeSystem)
		{
			TimeSystem = timeSystem;
		}

		#region IThread Members

		/// <inheritdoc cref="ITimeSystemExtensionPoint.TimeSystem" />
		public ITimeSystem TimeSystem { get; }

		/// <inheritdoc cref="IThread.Sleep(int)" />
		public void Sleep(int millisecondsTimeout)
		{
			System.Threading.Thread.Sleep(millisecondsTimeout);
		}

		/// <inheritdoc cref="IThread.Sleep(TimeSpan)" />
		public void Sleep(TimeSpan timeout)
		{
			System.Threading.Thread.Sleep(timeout);
		}

		#endregion
	}
}