using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Testing.FileSystem;

/// <summary>
///     Null object of an <see cref="IAccessControlStrategy" /> which does not restrict access in any way.
/// </summary>
public class NullAccessControlStrategy : IAccessControlStrategy
{
	/// <inheritdoc cref="IAccessControlStrategy.IsAccessGranted(string, IFileSystemExtensionContainer)" />
	public bool IsAccessGranted(string fullPath, IFileSystemExtensionContainer extensionContainer)
		=> true;
}
