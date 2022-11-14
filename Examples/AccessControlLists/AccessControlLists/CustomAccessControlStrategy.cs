using System;
using Testably.Abstractions.FileSystem;
using Testably.Abstractions.Testing.FileSystem;

namespace AccessControlLists;

public sealed class CustomAccessControlStrategy : IAccessControlStrategy
{
	private readonly Func<string, bool> _callback;

	public CustomAccessControlStrategy(Func<string, bool> callback)
	{
		_callback = callback;
	}

	/// <inheritdoc cref="CustomAccessControlStrategy.IsAccessGranted(string, IFileSystemExtensionContainer)" />
	public bool IsAccessGranted(string fullPath, IFileSystemExtensionContainer extensionContainer)
		=> _callback(fullPath);
}
