using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Testably.Abstractions.Testing;

public sealed partial class FileSystemMock
{
	private sealed class
		FileSystemWatcherFactoryMock : IFileSystem.IFileSystemWatcherFactory
	{
		private readonly FileSystemMock _fileSystem;

		internal FileSystemWatcherFactoryMock(FileSystemMock fileSystem)
		{
			_fileSystem = fileSystem;
		}

		#region IFileSystemWatcherFactory Members

		/// <inheritdoc cref="IFileSystem.IFileSystemExtensionPoint.FileSystem" />
		public IFileSystem FileSystem
			=> _fileSystem;

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcherFactory.New()" />
		public IFileSystem.IFileSystemWatcher New()
			=> FileSystemWatcherMock.New(_fileSystem);

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcherFactory.New(string)" />
		public IFileSystem.IFileSystemWatcher New(string path)
		{
			FileSystemWatcherMock fileSystemWatcherMock =
				FileSystemWatcherMock.New(_fileSystem);
			fileSystemWatcherMock.Path = path;
			return fileSystemWatcherMock;
		}

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcherFactory.New(string, string)" />
		public IFileSystem.IFileSystemWatcher New(string path, string filter)
		{
			FileSystemWatcherMock fileSystemWatcherMock =
				FileSystemWatcherMock.New(_fileSystem);
			fileSystemWatcherMock.Path = path;
			fileSystemWatcherMock.Filter = filter;
			return fileSystemWatcherMock;
		}

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcherFactory.Wrap(FileSystemWatcher)" />
		[return: NotNullIfNotNull("fileSystemWatcher")]
		// ReSharper disable once ReturnTypeCanBeNotNullable
		public IFileSystem.IFileSystemWatcher? Wrap(FileSystemWatcher? fileSystemWatcher)
		{
			if (fileSystemWatcher == null)
			{
				return null;
			}

			FileSystemWatcherMock fileSystemWatcherMock =
				FileSystemWatcherMock.New(_fileSystem);
			fileSystemWatcherMock.Path = fileSystemWatcher.Path;
#if FEATURE_FILESYSTEMWATCHER_ADVANCED
			foreach (var filter in fileSystemWatcher.Filters)
			{
				fileSystemWatcherMock.Filters.Add(filter);
			}
#else
			fileSystemWatcherMock.Filter = fileSystemWatcher.Filter;
#endif
			fileSystemWatcherMock.NotifyFilter = fileSystemWatcher.NotifyFilter;
			fileSystemWatcherMock.IncludeSubdirectories = fileSystemWatcher.IncludeSubdirectories;
			fileSystemWatcherMock.InternalBufferSize = fileSystemWatcher.InternalBufferSize;
			fileSystemWatcherMock.EnableRaisingEvents = fileSystemWatcher.EnableRaisingEvents;
			return fileSystemWatcherMock;
		}

#endregion
	}
}