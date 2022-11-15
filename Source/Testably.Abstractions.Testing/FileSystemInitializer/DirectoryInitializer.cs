using System;

namespace Testably.Abstractions.Testing.FileSystemInitializer;

internal sealed class DirectoryInitializer<TFileSystem>
	: Initializer<TFileSystem>,
		IFileSystemDirectoryInitializer<TFileSystem>
	where TFileSystem : IFileSystem
{
	public DirectoryInitializer(Initializer<TFileSystem> initializer,
		IDirectoryInfo directory)
		: base(initializer)
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
		Initializer<TFileSystem> initializer = new(this, Directory);
		subdirectoryInitializer.Invoke(initializer);
		return this;
	}

	#endregion
}
