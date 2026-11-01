#if FEATURE_FILESYSTEM_UNIXFILEMODE
using System.IO;
using Testably.Abstractions.Helpers;

namespace Testably.Abstractions.Testing.FileSystem;

/// <summary>
///     Null object of an <see cref="UnixFileModeStrategy" /> which only verifies that the unix file mode is not
///     <see cref="UnixFileMode.None" />.
/// </summary>
public class NullUnixFileModeStrategy : UnixFileModeStrategy
{
	/// <inheritdoc cref="UnixFileModeStrategy.IsAccessGranted(string, IFileSystemExtensibility, UnixFileMode, AccessRequest)" />
	public override bool IsAccessGranted(string fullPath, IFileSystemExtensibility extensibility,
		UnixFileMode mode, AccessRequest request)
		=> mode != UnixFileMode.None;
}
#endif
