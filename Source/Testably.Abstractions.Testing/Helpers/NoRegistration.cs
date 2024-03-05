using System;

namespace Testably.Abstractions.Testing.Helpers;

internal sealed class NoRegistration : IDisposable
{
	/// <inheritdoc />
	public void Dispose()
	{
		// Do nothing
	}
}
