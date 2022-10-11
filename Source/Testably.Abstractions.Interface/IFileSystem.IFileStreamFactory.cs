using System.IO;

namespace Testably.Abstractions;

public partial interface IFileSystem
{
	/// <summary>
	///     Factory for abstracting creation of <see cref="System.IO.FileStream" />.
	/// </summary>
	public interface IFileStreamFactory : IFileSystemExtensionPoint
	{
		/// <inheritdoc cref="System.IO.FileStream(string, FileMode)" />
		FileSystemStream New(string path, FileMode mode);

		/// <inheritdoc cref="System.IO.FileStream(string, FileMode, FileAccess)" />
		FileSystemStream New(string path, FileMode mode, FileAccess access);

		/// <inheritdoc cref="System.IO.FileStream(string, FileMode, FileAccess, FileShare)" />
		FileSystemStream New(string path, FileMode mode, FileAccess access,
		                     FileShare share);

		/// <inheritdoc cref="System.IO.FileStream(string, FileMode, FileAccess, FileShare, int)" />
		FileSystemStream New(string path, FileMode mode, FileAccess access,
		                     FileShare share, int bufferSize);

		/// <inheritdoc cref="System.IO.FileStream(string, FileMode, FileAccess, FileShare, int, bool)" />
		FileSystemStream New(string path, FileMode mode, FileAccess access,
		                     FileShare share, int bufferSize, bool useAsync);

		/// <inheritdoc cref="System.IO.FileStream(string, FileMode, FileAccess, FileShare, int, FileOptions)" />
		FileSystemStream New(string path, FileMode mode, FileAccess access,
		                     FileShare share, int bufferSize, FileOptions options);

#if FEATURE_FILESYSTEM_STREAM_OPTIONS
		/// <inheritdoc cref="System.IO.FileStream(string, FileStreamOptions)" />
		FileSystemStream New(string path, FileStreamOptions options);
#endif

		/// <summary>
		///     Wraps the <paramref name="fileStream" /> to the testable <see cref="FileSystemStream" />.
		/// </summary>
		FileSystemStream Wrap(FileStream fileStream);
	}
}