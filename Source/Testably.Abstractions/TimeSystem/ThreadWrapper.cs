using System;
using System.Threading;

namespace Testably.Abstractions.TimeSystem;

internal sealed class ThreadWrapper : IThread
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
		Thread.Sleep(millisecondsTimeout);
	}

	/// <inheritdoc cref="IThread.Sleep(TimeSpan)" />
	public void Sleep(TimeSpan timeout)
	{
		Thread.Sleep(timeout);
	}

	#endregion
}
