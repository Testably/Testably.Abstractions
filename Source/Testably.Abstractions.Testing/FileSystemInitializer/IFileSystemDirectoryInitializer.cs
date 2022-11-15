using System;

namespace Testably.Abstractions.Testing.FileSystemInitializer;

/// <summary>
///     Initializes a directory in the <see cref="IFileSystem" /> with test data.
/// </summary>
public interface IFileSystemDirectoryInitializer<out TFileSystem>
	: IFileSystemInitializer<TFileSystem>
	where TFileSystem : IFileSystem
{
	/// <summary>
	///     The directory to initialize.
	/// </summary>
	public IDirectoryInfo Directory { get; }

	/// <summary>
	///     Initializes the subdirectory in the <see cref="IFileSystem" /> with test data.
	/// </summary>
	public IFileSystemDirectoryInitializer<TFileSystem> Initialized(
		Action<IFileSystemInitializer<TFileSystem>> subdirectoryInitializer);
}
