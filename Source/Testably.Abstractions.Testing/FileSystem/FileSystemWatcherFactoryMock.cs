using System.Diagnostics.CodeAnalysis;
using System.IO;
using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.FileSystem;

internal sealed class
	FileSystemWatcherFactoryMock : IFileSystemWatcherFactory
{
	private readonly MockFileSystem _fileSystem;

	internal FileSystemWatcherFactoryMock(MockFileSystem fileSystem)
	{
		_fileSystem = fileSystem;
	}

	#region IFileSystemWatcherFactory Members

	/// <inheritdoc cref="IFileSystemEntity.FileSystem" />
	public IFileSystem FileSystem
		=> _fileSystem;

	/// <inheritdoc cref="IFileSystemWatcherFactory.New()" />
	public IFileSystemWatcher New()
		=> FileSystemWatcherMock.New(_fileSystem);

	/// <inheritdoc cref="IFileSystemWatcherFactory.New(string)" />
	public IFileSystemWatcher New(string path)
	{
		FileSystemWatcherMock fileSystemWatcherMock =
			FileSystemWatcherMock.New(_fileSystem);
		fileSystemWatcherMock.Path = path.EnsureValidArgument(_fileSystem);
		return fileSystemWatcherMock;
	}

	/// <inheritdoc cref="IFileSystemWatcherFactory.New(string, string)" />
	public IFileSystemWatcher New(string path, string filter)
	{
		FileSystemWatcherMock fileSystemWatcherMock =
			FileSystemWatcherMock.New(_fileSystem);
		fileSystemWatcherMock.Path = path.EnsureValidArgument(_fileSystem);
		fileSystemWatcherMock.Filter = filter;
		return fileSystemWatcherMock;
	}

	/// <inheritdoc cref="IFileSystemWatcherFactory.Wrap(FileSystemWatcher)" />
	[return: NotNullIfNotNull("fileSystemWatcher")]
	// ReSharper disable once ReturnTypeCanBeNotNullable
	public IFileSystemWatcher? Wrap(FileSystemWatcher? fileSystemWatcher)
	{
		if (fileSystemWatcher == null)
		{
			return null;
		}

		FileSystemWatcherMock fileSystemWatcherMock =
			FileSystemWatcherMock.New(_fileSystem);
		fileSystemWatcherMock.Path = fileSystemWatcher.Path;
#if FEATURE_FILESYSTEMWATCHER_ADVANCED
		foreach (string filter in fileSystemWatcher.Filters)
		{
			fileSystemWatcherMock.Filters.Add(filter);
		}
#else
		fileSystemWatcherMock.Filter = fileSystemWatcher.Filter;
#endif
		fileSystemWatcherMock.NotifyFilter = fileSystemWatcher.NotifyFilter;
		fileSystemWatcherMock.IncludeSubdirectories =
			fileSystemWatcher.IncludeSubdirectories;
		fileSystemWatcherMock.InternalBufferSize =
			fileSystemWatcher.InternalBufferSize;
		fileSystemWatcherMock.EnableRaisingEvents =
			fileSystemWatcher.EnableRaisingEvents;
		return fileSystemWatcherMock;
	}

	#endregion
}
