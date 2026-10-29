using Testably.Abstractions.Helpers;

namespace Testably.Abstractions.Testing.FileSystem;

/// <summary>
///     Null object of an <see cref="AccessControlStrategy" /> which does not restrict access in any way.
/// </summary>
public class NullAccessControlStrategy : AccessControlStrategy
{
	/// <inheritdoc cref="AccessControlStrategy.IsAccessGranted(string, IFileSystemExtensibility, AccessRequest)" />
	public override bool IsAccessGranted(string fullPath, IFileSystemExtensibility extensibility,
		AccessRequest request)
		=> true;
}
