using System.IO.Abstractions;
using System;
using System.IO;
using Microsoft.Win32.SafeHandles;

namespace Testably.Abstractions
{
/// <summary>
///     A factory for the creation of wrappers for <see cref="FileStream" /> in a <see cref="IFileSystem" />.
/// </summary>
public interface IFileStreamFactory : IFileSystemEntity
{
	/// <inheritdoc cref="FileStream(SafeFileHandle, FileAccess)" />
	FileSystemStream New(SafeFileHandle handle, FileAccess access);

	/// <inheritdoc cref="FileStream(SafeFileHandle ,FileAccess, int)" />
	FileSystemStream New(SafeFileHandle handle, FileAccess access, int bufferSize);

	/// <inheritdoc cref="FileStream(SafeFileHandle, FileAccess, int, bool)" />
	FileSystemStream New(SafeFileHandle handle, FileAccess access, int bufferSize, bool isAsync);

	/// <inheritdoc cref="FileStream(string, FileMode)" />
	FileSystemStream New(string path, FileMode mode);

	/// <inheritdoc cref="FileStream(string, FileMode, FileAccess)" />
	FileSystemStream New(string path, FileMode mode, FileAccess access);

	/// <inheritdoc cref="FileStream(string, FileMode, FileAccess, FileShare)" />
	FileSystemStream New(string path, FileMode mode, FileAccess access,
		FileShare share);

	/// <inheritdoc cref="FileStream(string, FileMode, FileAccess, FileShare, int)" />
	FileSystemStream New(string path, FileMode mode, FileAccess access,
		FileShare share, int bufferSize);

	/// <inheritdoc cref="FileStream(string, FileMode, FileAccess, FileShare, int, bool)" />
	FileSystemStream New(string path, FileMode mode, FileAccess access,
		FileShare share, int bufferSize, bool useAsync);

	/// <inheritdoc cref="FileStream(string, FileMode, FileAccess, FileShare, int, FileOptions)" />
	FileSystemStream New(string path, FileMode mode, FileAccess access,
		FileShare share, int bufferSize, FileOptions options);

#if FEATURE_FILESYSTEM_STREAM_OPTIONS
	    /// <inheritdoc cref="FileStream(string, FileStreamOptions)" />
	    FileSystemStream New(string path, FileStreamOptions options);
#endif

	/// <summary>
	///     Wraps the <paramref name="fileStream" /> to the testable <see cref="FileSystemStream" />.
	/// </summary>
	FileSystemStream Wrap(FileStream fileStream);
}
}

namespace System.IO.Abstractions
{
	/// <summary>
	///     Backwards-compatibility alias for <see cref="Testably.Abstractions.IFileStreamFactory" />.
	/// </summary>
	public interface IFileStreamFactory : Testably.Abstractions.IFileStreamFactory
	{
	}
}