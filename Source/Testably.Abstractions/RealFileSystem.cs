using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions;

/// <summary>
///     Default implementation for file-related system dependencies.
///     <para />
///     Implements <seealso cref="IFileSystem" />
/// </summary>
public sealed class RealFileSystem : IFileSystem
{
	/// <summary>
	///     Initializes a new instance of <see cref="RealFileSystem" />
	///     which wraps the file-related system dependencies from <see cref="IFileSystem" />.
	/// </summary>
	public RealFileSystem()
	{
		Directory = new DirectoryWrapper(this);
		DirectoryInfo = new DirectoryInfoFactory(this);
		DriveInfo = new DriveInfoFactory(this);
		File = new FileWrapper(this);
		FileInfo = new FileInfoFactory(this);
		FileStream = new FileStreamFactory(this);
		FileSystemWatcher = new FileSystemWatcherFactory(this);
		Path = new PathWrapper(this);
	}

	#region IFileSystem Members

	/// <inheritdoc cref="IFileSystem.Directory" />
	public IDirectory Directory { get; }

	/// <inheritdoc cref="IFileSystem.DirectoryInfo" />
	public IDirectoryInfoFactory DirectoryInfo { get; }

	/// <inheritdoc cref="IFileSystem.DriveInfo" />
	public IDriveInfoFactory DriveInfo { get; }

	/// <inheritdoc cref="IFileSystem.File" />
	public IFile File { get; }

	/// <inheritdoc cref="IFileSystem.FileInfo" />
	public IFileInfoFactory FileInfo { get; }

	/// <inheritdoc cref="IFileSystem.FileStream" />
	public IFileStreamFactory FileStream { get; }

	/// <inheritdoc cref="IFileSystem.FileSystemWatcher" />
	public IFileSystemWatcherFactory FileSystemWatcher { get; }

	/// <inheritdoc cref="IFileSystem.Path" />
	public IPath Path { get; }

	#endregion
}
