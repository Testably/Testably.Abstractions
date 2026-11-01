using Testably.Abstractions.Helpers;

namespace Testably.Abstractions.Testing.FileSystem;

/// <summary>
///     The strategy to simulate access control (ACL) in the <see cref="MockFileSystem" />.
/// </summary>
public abstract class AccessControlStrategy
{
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
