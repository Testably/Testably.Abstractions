using System;

namespace Testably.Abstractions.Testing.Helpers;

internal sealed class NoOpDisposable : IDisposable
{
	/// <inheritdoc cref="IDisposable.Dispose()" />
	public void Dispose()
	{
		// Do nothing
	}
}
