using System.IO;
using Testably.Abstractions.Helpers;

namespace Testably.Abstractions.Testing.FileSystem;

/// <summary>
///     Base class for a strategy to simulate access control (ACL) in the <see cref="MockFileSystem" />.
/// </summary>
/// <remarks>
///     Prefer deriving from this class over implementing <see cref="IAccessControlStrategy" /> directly,
///     as it receives the <see cref="AccessRequest" />.
/// </remarks>
public abstract class AccessControlStrategy : IAccessControlStrategy
{
	#region IAccessControlStrategy Members

	/// <inheritdoc cref="IAccessControlStrategy.IsAccessGranted(string, IFileSystemExtensibility)" />
	/// <remarks>
	///     Requests <see cref="FileAccess.ReadWrite" />, so that access is never granted by accident.
	/// </remarks>
	public bool IsAccessGranted(string fullPath, IFileSystemExtensibility extensibility)
		=> IsAccessGranted(fullPath, extensibility, new AccessRequest(FileAccess.ReadWrite));

	#endregion

	/// <summary>
	///     Implements a custom strategy to simulate access control (ACL) in the <see cref="MockFileSystem" />.
	/// </summary>
	/// <param name="fullPath">The full path to the file or directory.</param>
	/// <param name="extensibility">The extension container to store additional data.</param>
	/// <param name="request">The requested access to the file or directory.</param>
	/// <returns>
	///     <see langword="true" /> if the requested access should be granted, otherwise <see langword="false" />.
	/// </returns>
	public abstract bool IsAccessGranted(string fullPath,
		IFileSystemExtensibility extensibility,
		AccessRequest request);
}
