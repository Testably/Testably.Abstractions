using System.IO;
using Testably.Abstractions.Helpers;

namespace Testably.Abstractions.Testing.FileSystem;

/// <summary>
///     Base class for a strategy to simulate access control (ACL) in the <see cref="MockFileSystem" />.
/// </summary>
/// <remarks>
///     Prefer deriving from this class over implementing <see cref="IAccessControlStrategy" /> directly,
///     as it receives the requested <see cref="FileAccess" />.
/// </remarks>
public abstract class AccessControlStrategy : IAccessControlStrategy
{
	#region IAccessControlStrategy Members

	/// <inheritdoc cref="IAccessControlStrategy.IsAccessGranted(string, IFileSystemExtensibility)" />
	/// <remarks>
	///     Requests <see cref="FileAccess.ReadWrite" />, so that access is never granted by accident.
	/// </remarks>
	public bool IsAccessGranted(string fullPath, IFileSystemExtensibility extensibility)
		=> IsAccessGranted(fullPath, extensibility, FileAccess.ReadWrite);

	#endregion

	/// <summary>
	///     Implements a custom strategy to simulate access control (ACL) in the <see cref="MockFileSystem" />.
	/// </summary>
	/// <param name="fullPath">The full path to the file or directory.</param>
	/// <param name="extensibility">The extension container to store additional data.</param>
	/// <param name="requestedAccess">
	///     The requested <see cref="FileAccess" /> of the file or directory.<br />
	///     Enumerating a directory requests <see cref="FileAccess.Read" />. As <see cref="FileAccess" /> is a flags enum,
	///     check write access with <c>requestedAccess.HasFlag(FileAccess.Write)</c> to also cover
	///     <see cref="FileAccess.ReadWrite" />.
	/// </param>
	/// <returns>
	///     <see langword="true" /> if the requested access should be granted, otherwise <see langword="false" />.
	/// </returns>
	public abstract bool IsAccessGranted(string fullPath,
		IFileSystemExtensibility extensibility,
		FileAccess requestedAccess);
}
