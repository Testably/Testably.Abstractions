using System;

namespace Testably.Abstractions;

public partial interface ITimeSystem
{
	/// <summary>
	///     Abstractions for <see cref="System.Threading.Thread" />.
	/// </summary>
	interface IThread : ITimeSystemExtensionPoint
	{
		/// <inheritdoc cref="System.Threading.Thread.Sleep(int)" />
		void Sleep(int millisecondsTimeout);

		/// <inheritdoc cref="System.Threading.Thread.Sleep(TimeSpan)" />
		void Sleep(TimeSpan timeout);
	}
}