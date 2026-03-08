using System;
using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.Initializer;

internal sealed class DirectoryInitializer<TFileSystem>
	: FileSystemInitializer<TFileSystem>,
		IFileSystemDirectoryInitializer<TFileSystem>
	where TFileSystem : IFileSystem
{
	public DirectoryInitializer(FileSystemInitializer<TFileSystem> initializer,
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
		using IDisposable release = FileSystem.IgnoreStatistics();
		FileSystemInitializer<TFileSystem> initializer =
			new(this, FileSystem.Path.Combine(BasePath, Directory.Name));
		subdirectoryInitializer.Invoke(initializer);
		return this;
	}

	#endregion
}
