using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Testably.Abstractions.FileSystem;

internal sealed class FileSystemWatcherFactory : IFileSystemWatcherFactory
{
	internal FileSystemWatcherFactory(RealFileSystem fileSystem)
	{
		FileSystem = fileSystem;
	}

	#region IFileSystemWatcherFactory Members

	/// <inheritdoc cref="IFileSystemExtensionPoint.FileSystem" />
	public IFileSystem FileSystem { get; }

	/// <inheritdoc cref="IFileSystemWatcherFactory.New()" />
	public IFileSystemWatcher New()
		=> FileSystemWatcherWrapper.FromFileSystemWatcher(
			new FileSystemWatcher(),
			FileSystem);

	/// <inheritdoc cref="IFileSystemWatcherFactory.New(string)" />
	public IFileSystemWatcher New(string path)
		=> FileSystemWatcherWrapper.FromFileSystemWatcher(
			new FileSystemWatcher(path),
			FileSystem);

	/// <inheritdoc cref="IFileSystemWatcherFactory.New(string, string)" />
	public IFileSystemWatcher New(string path, string filter)
		=> FileSystemWatcherWrapper.FromFileSystemWatcher(
			new FileSystemWatcher(path, filter),
			FileSystem);

	/// <inheritdoc cref="IFileSystemWatcherFactory.Wrap(FileSystemWatcher)" />
	[return: NotNullIfNotNull("fileSystemWatcher")]
	public IFileSystemWatcher? Wrap(FileSystemWatcher? fileSystemWatcher)
		=> FileSystemWatcherWrapper.FromFileSystemWatcher(
			fileSystemWatcher,
			FileSystem);

	#endregion
}
