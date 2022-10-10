﻿using System;

namespace Testably.Abstractions.Testing;

public static partial class FileSystemInitializer
{
	private sealed class DirectoryInitializer<TFileSystem>
		: Initializer<TFileSystem>,
			IFileSystemDirectoryInitializer<TFileSystem>
		where TFileSystem : IFileSystem
	{
		public DirectoryInitializer(Initializer<TFileSystem> initializer,
		                            IFileSystem.IDirectoryInfo directory)
			: base(initializer)
		{
			Directory = directory;
		}

		#region IFileSystemDirectoryInitializer<TFileSystem> Members

		/// <inheritdoc cref="IFileSystemDirectoryInitializer{TFileSystem}.Directory" />
		public IFileSystem.IDirectoryInfo Directory { get; }

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
}