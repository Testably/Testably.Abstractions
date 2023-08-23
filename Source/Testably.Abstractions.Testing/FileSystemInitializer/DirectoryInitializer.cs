using System;

namespace Testably.Abstractions.Testing.FileSystemInitializer;

internal sealed class DirectoryInitializer<TFileSystem>
	: FileSystemInitializer<TFileSystem>,
		IFileSystemDirectoryInitializer<TFileSystem>
	where TFileSystem : IFileSystem
{
	public DirectoryInitializer(FileSystemInitializer<TFileSystem> initializer,
		IDirectoryInfo directory)
		: base(initializer, directory)
	{
		Directory = directory;
	}

	#region IFileSystemDirectoryInitializer<TFileSystem> Members

	/// <inheritdoc cref="IFileSystemDirectoryInitializer{TFileSystem}.Directory" />
	public IDirectoryInfo Directory { get; }

	/// <inheritdoc
	///     cref="IFileSystemDirectoryInitializer{TFileSystem}.Initialized(Action{IFileSystemInitializer{TFileSystem}})" />
	public IFileSystemDirectoryInitializer<TFileSystem> Initialized(
		Action<IFileSystemInitializer<TFileSystem>> subdirectoryInitializer)
	{
		FileSystemInitializer<TFileSystem> initializer = new(this, Directory);
		subdirectoryInitializer.Invoke(initializer);
		return this;
	}

	#endregion
}
