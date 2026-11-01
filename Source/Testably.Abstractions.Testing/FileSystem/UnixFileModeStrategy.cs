#if FEATURE_FILESYSTEM_UNIXFILEMODE
using System.IO;
using Testably.Abstractions.Helpers;

namespace Testably.Abstractions.Testing.FileSystem;

/// <summary>
///     The strategy to simulate working with the <see cref="UnixFileMode" /> in the <see cref="MockFileSystem" />.
/// </summary>
public abstract class UnixFileModeStrategy
{
	/// <summary>
	///     Implements a custom strategy to simulate working with the <see cref="UnixFileMode" /> in the
	///     <see cref="MockFileSystem" />.
	/// </summary>
	/// <param name="fullPath">The full path to the file or directory.</param>
	/// <param name="extensibility">The extension container to store additional data.</param>
	/// <param name="mode">The <see cref="UnixFileMode" /> of the file or directory.</param>
	/// <param name="request">The requested access to the file or directory.</param>
	/// <returns>
	///     <see langword="true" /> if the requested access should be granted, otherwise <see langword="false" />.
	/// </returns>
	/// <remarks>
	///     Deleting a file or directory is not checked by this strategy.
	/// </remarks>
	public abstract bool IsAccessGranted(string fullPath,
		IFileSystemExtensibility extensibility,
		UnixFileMode mode,
		AccessRequest request);

	/// <summary>
	///     Callback executed when setting the <see cref="UnixFileMode" /> to allow storing additional data in
	///     <paramref name="extensibility" />.
	/// </summary>
	/// <remarks>
	///     Does nothing by default.
	/// </remarks>
	public virtual void OnSetUnixFileMode(string fullPath,
		IFileSystemExtensibility extensibility,
		UnixFileMode mode)
	{
		// Do nothing by default
	}
}
#endif
