using System;

namespace Testably.Abstractions.Testing;

public static partial class FileSystemInitializer
{
	private sealed class FileInitializer<TFileSystem>
		: Initializer<TFileSystem>,
			IFileSystemFileInitializer<TFileSystem>
		where TFileSystem : IFileSystem
	{
		public FileInitializer(Initializer<TFileSystem> initializer,
		                       IFileSystem.IFileInfo file)
			: base(initializer)
		{
			File = file;
		}

		#region IFileSystemFileInitializer<TFileSystem> Members

		/// <inheritdoc cref="IFileSystemFileInitializer{TFileSystem}.File" />
		public IFileSystem.IFileInfo File { get; }

		/// <inheritdoc cref="IFileSystemFileInitializer{TFileSystem}.Which(Action{IFileManipulator})" />
		public IFileSystemFileInitializer<TFileSystem> Which(
			Action<IFileManipulator> fileManipulation)
		{
			FileManipulator fileManipulator = new(FileSystem, File);
			fileManipulation.Invoke(fileManipulator);
			return this;
		}

		#endregion

		private sealed class FileManipulator : IFileManipulator
		{
			internal FileManipulator(IFileSystem fileSystem, IFileSystem.IFileInfo file)
			{
				FileSystem = fileSystem;
				File = file;
			}

			#region IFileManipulator Members

			/// <inheritdoc cref="IFileManipulator.File" />
			public IFileSystem.IFileInfo File { get; }

			/// <inheritdoc cref="IFileSystem.IFileSystemExtensionPoint.FileSystem" />
			public IFileSystem FileSystem { get; }

			/// <inheritdoc cref="IFileManipulator.HasBytesContent" />
			public IFileManipulator HasBytesContent(byte[] bytes)
			{
				FileSystem.File.WriteAllBytes(File.FullName, bytes);
				return this;
			}

			/// <inheritdoc cref="IFileManipulator.HasStringContent" />
			public IFileManipulator HasStringContent(string contents)
			{
				FileSystem.File.WriteAllText(File.FullName, contents);
				return this;
			}

			#endregion
		}
	}
}