using System;

namespace Testably.Abstractions.Testing;

/// <summary>
///     Initializes the <see cref="IFileSystem" /> with test data.
/// </summary>
public static partial class FileSystemInitializer
{
	/// <summary>
	///     Initializes the <see cref="IFileSystem" /> in the working directory with test data.
	/// </summary>
	public static IFileSystemInitializer<TFileSystem> Initialize<TFileSystem>(
		this TFileSystem fileSystem)
		where TFileSystem : IFileSystem
		=> fileSystem.InitializeIn(".");

	/// <summary>
	///     Initializes the <see cref="IFileSystem" /> in the <paramref name="basePath" /> with test data.
	/// </summary>
	public static IFileSystemInitializer<TFileSystem> InitializeIn<TFileSystem>(
		this TFileSystem fileSystem,
		string basePath)
		where TFileSystem : IFileSystem
	{
		fileSystem.Directory.CreateDirectory(basePath);
		if (fileSystem is FileSystemMock)
		{
			//TODO: Check if this could be activated for all file systems
			fileSystem.Directory.SetCurrentDirectory(basePath);
		}
		return new Initializer<TFileSystem>(fileSystem, basePath);
	}

	/// <summary>
	///     Sets the current directory to a new temporary directory.<br />
	///     <see cref="IFileSystem.IDirectory.GetCurrentDirectory()" /> and all relative paths will use this directory.
	/// </summary>
	/// <param name="fileSystem">The file system.</param>
	/// <param name="logger">(optional) A callback to log the cleanup process.</param>
	/// <returns>
	///     A <see cref="IDirectoryCleaner" /> that will
	///     force delete all content in the temporary directory on dispose.
	/// </returns>
	public static IDirectoryCleaner SetCurrentDirectoryToEmptyTemporaryDirectory(
		this IFileSystem fileSystem, Action<string>? logger = null)
	{
		return new DirectoryCleaner(fileSystem, logger);
	}
}