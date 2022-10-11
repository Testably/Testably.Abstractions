﻿using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Testably.Abstractions;

public partial interface IFileSystem
{
	/// <summary>
	///     Factory for abstracting the creation of <see cref="System.IO.FileSystemWatcher" />.
	/// </summary>
	public interface IFileSystemWatcherFactory : IFileSystemExtensionPoint
	{
		/// <inheritdoc cref="System.IO.FileSystemWatcher()" />
		IFileSystemWatcher New();

		/// <inheritdoc cref="System.IO.FileSystemWatcher(string)" />
		IFileSystemWatcher New(string path);

		/// <inheritdoc cref="System.IO.FileSystemWatcher(string, string)" />
		IFileSystemWatcher New(string path, string filter);

		/// <summary>
		///     Wraps the <paramref name="fileSystemWatcher" /> to the testable interface <see cref="IFileSystemWatcher" />.
		/// </summary>
		[return: NotNullIfNotNull("fileSystemWatcher")]
		IFileSystemWatcher? Wrap(FileSystemWatcher? fileSystemWatcher);
	}
}