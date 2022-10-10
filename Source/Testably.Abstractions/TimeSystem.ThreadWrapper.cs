using System;

namespace Testably.Abstractions;

public sealed partial class TimeSystem
{
	private sealed class ThreadWrapper : ITimeSystem.IThread
	{
		internal ThreadWrapper(TimeSystem timeSystem)
		{
			TimeSystem = timeSystem;
		}

		#region IThread Members

		/// <inheritdoc cref="ITimeSystem.ITimeSystemExtensionPoint.TimeSystem" />
		public ITimeSystem TimeSystem { get; }

		/// <inheritdoc cref="ITimeSystem.IThread.Sleep(int)" />
		public void Sleep(int millisecondsTimeout)
		{
			System.Threading.Thread.Sleep(millisecondsTimeout);
		}

		/// <inheritdoc cref="ITimeSystem.IThread.Sleep(TimeSpan)" />
		public void Sleep(TimeSpan timeout)
		{
			System.Threading.Thread.Sleep(timeout);
		}

		#endregion
	}
}