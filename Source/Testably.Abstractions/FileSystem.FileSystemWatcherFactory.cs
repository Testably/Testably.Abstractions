using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Testably.Abstractions;

public sealed partial class FileSystem
{
	private sealed class FileSystemWatcherFactory : IFileSystem.IFileSystemWatcherFactory
	{
		internal FileSystemWatcherFactory(FileSystem fileSystem)
		{
			FileSystem = fileSystem;
		}

		#region IFileSystemWatcherFactory Members

		/// <inheritdoc cref="IFileSystem.IFileSystemExtensionPoint.FileSystem" />
		public IFileSystem FileSystem { get; }

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcherFactory.New()" />
		public IFileSystem.IFileSystemWatcher New()
			=> FileSystemWatcherWrapper.FromFileSystemWatcher(
				new FileSystemWatcher(),
				FileSystem);

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcherFactory.New(string)" />
		public IFileSystem.IFileSystemWatcher New(string path)
			=> FileSystemWatcherWrapper.FromFileSystemWatcher(
				new FileSystemWatcher(path),
				FileSystem);

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcherFactory.New(string, string)" />
		public IFileSystem.IFileSystemWatcher New(string path, string filter)
			=> FileSystemWatcherWrapper.FromFileSystemWatcher(
				new FileSystemWatcher(path, filter),
				FileSystem);

		/// <inheritdoc cref="IFileSystem.IFileSystemWatcherFactory.Wrap(FileSystemWatcher)" />
		[return: NotNullIfNotNull("fileSystemWatcher")]
		public IFileSystem.IFileSystemWatcher? Wrap(FileSystemWatcher? fileSystemWatcher)
			=> FileSystemWatcherWrapper.FromFileSystemWatcher(
				fileSystemWatcher,
				FileSystem);

		#endregion
	}
}