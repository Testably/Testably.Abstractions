using System;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Testing.FileSystem;

/// <summary>
///     Default implementation of an <see cref="IAccessControlStrategy" /> which uses a callback to determine if access
///     should be granted.
/// </summary>
public class DefaultAccessControlStrategy : IAccessControlStrategy
{
	private readonly Func<string, IFileSystemExtensionContainer, bool> _callback;

	/// <summary>
	///     Initializes a new instance of <see cref="DefaultAccessControlStrategy" /> which takes a callback to determine if
	///     access should be granted.
	/// </summary>
	public DefaultAccessControlStrategy(
		Func<string, IFileSystemExtensionContainer, bool> callback)
	{
		_callback = callback ?? throw new ArgumentNullException(nameof(callback));
	}

	/// <inheritdoc cref="IAccessControlStrategy.IsAccessGranted(string, IFileSystemExtensionContainer)" />
	public bool IsAccessGranted(string fullPath,
	                            IFileSystemExtensionContainer extensionContainer)
		=> _callback(fullPath, extensionContainer);
}