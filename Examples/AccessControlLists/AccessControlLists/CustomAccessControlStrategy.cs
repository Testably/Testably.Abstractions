using System;
using Testably.Abstractions.Helpers;
using Testably.Abstractions.Testing.FileSystem;

namespace Testably.Abstractions.Examples.AccessControlLists;

public sealed class CustomAccessControlStrategy : IAccessControlStrategy
{
	private readonly Func<string, bool> _callback;

	public CustomAccessControlStrategy(Func<string, bool> callback)
	{
		_callback = callback;
	}

	/// <inheritdoc cref="CustomAccessControlStrategy.IsAccessGranted(string, IFileSystemExtensibility)" />
	public bool IsAccessGranted(string fullPath, IFileSystemExtensibility extensibility)
		=> _callback(fullPath);
}
