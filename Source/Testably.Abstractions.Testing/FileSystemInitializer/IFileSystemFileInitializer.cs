using System;
using Testably.Abstractions.FileSystem;

namespace Testably.Abstractions.Testing.FileSystemInitializer;

/// <summary>
///     Initializes a file in the <see cref="IFileSystem" /> with test data.
/// </summary>
public interface IFileSystemFileInitializer<out TFileSystem>
	: IFileSystemInitializer<TFileSystem>
	where TFileSystem : IFileSystem
{
	/// <summary>
	///     The file to initialize.
	/// </summary>
	public IFileInfo File { get; }

	/// <summary>
	///     Manipulates the <see cref="File" /> in the <see cref="IFileSystem" /> with test data.
	/// </summary>
	public IFileSystemFileInitializer<TFileSystem> Which(
		Action<IFileManipulator> fileManipulation);
}