namespace Testably.Abstractions.Testing.FileSystem;

/// <summary>
///     Null object of an <see cref="IAccessControlStrategy" /> which does not restrict access in any way.
/// </summary>
public class NullAccessControlStrategy : IAccessControlStrategy
{
	/// <inheritdoc cref="IAccessControlStrategy.IsAccessGranted(string, IFileSystemExtensibility)" />
	public bool IsAccessGranted(string fullPath, IFileSystemExtensibility extensibility)
		=> true;
}
