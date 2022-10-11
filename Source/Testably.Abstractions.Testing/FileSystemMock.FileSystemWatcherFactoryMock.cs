using System;
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
			FileSystemWatcherMock fileSystemWatcher =
				FileSystemWatcherMock.New(_fileSystem);
			fileSystemWatcher.Path = path;
			return fileSystemWatcher;
		}

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcherFactory.New(string, string)" />
		public IFileSystem.IFileSystemWatcher New(string path, string filter)
		{
			FileSystemWatcherMock fileSystemWatcher =
				FileSystemWatcherMock.New(_fileSystem);
			fileSystemWatcher.Path = path;
			fileSystemWatcher.Filter = filter;
			return fileSystemWatcher;
		}

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcherFactory.Wrap(FileSystemWatcher)" />
		[return: NotNullIfNotNull("fileSystemWatcher")]
		// ReSharper disable once ReturnTypeCanBeNotNullable
		public IFileSystem.IFileSystemWatcher? Wrap(FileSystemWatcher? fileSystemWatcher)
			=> throw new NotSupportedException(
				"You cannot wrap an existing FileSystemWatcher in the FileSystemMock instance!");

		#endregion
	}
}