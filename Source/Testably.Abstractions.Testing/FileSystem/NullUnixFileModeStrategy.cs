#if FEATURE_FILESYSTEM_UNIXFILEMODE
using System.IO;
using Testably.Abstractions.Helpers;

namespace Testably.Abstractions.Testing.FileSystem;

/// <summary>
///     Null object of an <see cref="IAccessControlStrategy" /> which does not restrict access in any way.
/// </summary>
public class NullUnixFileModeStrategy : IUnixFileModeStrategy
{
	#region IUnixFileModeStrategy Members

	/// <inheritdoc cref="IUnixFileModeStrategy.IsAccessGranted(string, IFileSystemExtensibility, UnixFileMode, FileAccess)" />
	public bool IsAccessGranted(string fullPath, IFileSystemExtensibility extensibility,
		UnixFileMode mode, FileAccess requestedAccess)
		=> mode != UnixFileMode.None;

	/// <inheritdoc cref="IUnixFileModeStrategy.OnSetUnixFileMode(string, IFileSystemExtensibility, UnixFileMode)" />
	public void OnSetUnixFileMode(string fullPath, IFileSystemExtensibility extensibility,
		UnixFileMode mode)
	{
		// Do nothing
	}

	#endregion
}
#endif
