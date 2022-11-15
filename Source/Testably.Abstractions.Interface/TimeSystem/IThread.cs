using System;

namespace Testably.Abstractions.TimeSystem;

/// <summary>
///     Abstractions for <see cref="System.Threading.Thread" />.
/// </summary>
public interface IThread : ITimeSystemEntity
{
	/// <inheritdoc cref="System.Threading.Thread.Sleep(int)" />
	void Sleep(int millisecondsTimeout);

	/// <inheritdoc cref="System.Threading.Thread.Sleep(TimeSpan)" />
	void Sleep(TimeSpan timeout);
}
