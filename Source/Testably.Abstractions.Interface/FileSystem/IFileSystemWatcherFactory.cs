using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Testably.Abstractions.FileSystem;

/// <summary>
///     Factory for abstracting the creation of <see cref="FileSystemWatcher" />.
/// </summary>
public interface IFileSystemWatcherFactory : IFileSystemExtensionPoint
{
	/// <inheritdoc cref="FileSystemWatcher()" />
	IFileSystemWatcher New();

	/// <inheritdoc cref="FileSystemWatcher(string)" />
	IFileSystemWatcher New(string path);

	/// <inheritdoc cref="FileSystemWatcher(string, string)" />
	IFileSystemWatcher New(string path, string filter);

	/// <summary>
	///     Wraps the <paramref name="fileSystemWatcher" /> to the testable interface <see cref="IFileSystemWatcher" />.
	/// </summary>
	[return: NotNullIfNotNull("fileSystemWatcher")]
	IFileSystemWatcher? Wrap(FileSystemWatcher? fileSystemWatcher);
}
