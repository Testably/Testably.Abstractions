#if FEATURE_FILESYSTEM_UNIXFILEMODE
using System.IO;
using Testably.Abstractions.Helpers;

namespace Testably.Abstractions.Testing.FileSystem;

/// <summary>
///     Null object of an <see cref="IUnixFileModeStrategy" /> which only verifies that the unix file mode is not
///     <see cref="UnixFileMode.None" />.
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
