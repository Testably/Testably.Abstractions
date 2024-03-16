using System;

namespace Testably.Abstractions.Testing.Helpers;

internal sealed class NoOpDisposable : IDisposable
{
	#region IDisposable Members

	/// <inheritdoc cref="IDisposable.Dispose()" />
	public void Dispose()
	{
		// Do nothing
	}

	#endregion
}
