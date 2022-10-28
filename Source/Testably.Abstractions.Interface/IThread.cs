using System;

namespace Testably.Abstractions;

/// <summary>
///     Abstractions for <see cref="System.Threading.Thread" />.
/// </summary>
public interface IThread : ITimeSystemExtensionPoint
{
	/// <inheritdoc cref="System.Threading.Thread.Sleep(int)" />
	void Sleep(int millisecondsTimeout);

	/// <inheritdoc cref="System.Threading.Thread.Sleep(TimeSpan)" />
	void Sleep(TimeSpan timeout);
}