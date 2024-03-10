using System;

namespace Testably.Abstractions.Testing.Helpers;

internal sealed class NoOpDisposable : IDisposable
{
	/// <inheritdoc />
	public void Dispose()
	{
		// Do nothing
	}
}
