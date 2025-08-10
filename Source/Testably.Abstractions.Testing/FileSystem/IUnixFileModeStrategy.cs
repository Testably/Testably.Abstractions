#if FEATURE_FILESYSTEM_UNIXFILEMODE
using System.IO;
using Testably.Abstractions.Helpers;

namespace Testably.Abstractions.Testing.FileSystem;

/// <summary>
///     The strategy to simulate working with the <see cref="UnixFileMode" /> in the <see cref="MockFileSystem" />.
/// </summary>
public interface IUnixFileModeStrategy
{
	/// <summary>
	///     Implements a custom strategy to simulate working with the <see cref="UnixFileMode" /> in the
	///     <see cref="MockFileSystem" />.
	/// </summary>
	/// <param name="fullPath">The full path to the file or directory.</param>
	/// <param name="extensibility">The extension container to store additional data.</param>
	/// <param name="mode">The <see cref="UnixFileMode" /> of the file or directory.</param>
	/// <param name="requestedAccess">The requested <see cref="FileAccess" /> of the file or directory.</param>
	/// <returns>
	///     <see langword="true" /> if the requested access should be granted, otherwise <see langword="false" />.
	/// </returns>
	bool IsAccessGranted(string fullPath,
		IFileSystemExtensibility extensibility,
		UnixFileMode mode,
		FileAccess requestedAccess);

	/// <summary>
	///     Callback executed when setting the <see cref="UnixFileMode" /> to allow storing additional data in
	///     <paramref name="extensibility" />.
	/// </summary>
	void OnSetUnixFileMode(string fullPath,
		IFileSystemExtensibility extensibility,
		UnixFileMode mode);
}

#endif
