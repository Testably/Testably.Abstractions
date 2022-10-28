using System.Collections.Generic;
using Testably.Abstractions.Testing.Internal;

namespace Testably.Abstractions.Testing;

public static partial class FileSystemInitializer
{
	private class Initializer<TFileSystem>
		: IFileSystemInitializer<TFileSystem>
		where TFileSystem : IFileSystem
	{
		private readonly string _basePath;

		private readonly Dictionary<int, IFileSystemInfo>
			_initializedFileSystemInfos = new();

		public Initializer(TFileSystem fileSystem, string basePath)
		{
			_basePath = basePath;
			FileSystem = fileSystem;
		}

		protected Initializer(Initializer<TFileSystem> parent)
		{
			FileSystem = parent.FileSystem;
			_initializedFileSystemInfos = parent._initializedFileSystemInfos;
			_basePath = parent._basePath;
		}

		internal Initializer(Initializer<TFileSystem> parent,
		                     IDirectoryInfo subdirectory)
		{
			FileSystem = parent.FileSystem;
			_initializedFileSystemInfos = parent._initializedFileSystemInfos;
			_basePath = FileSystem.Path.Combine(parent._basePath, subdirectory.Name);
		}

		#region IFileSystemInitializer<TFileSystem> Members

		/// <inheritdoc cref="IFileSystemInitializer{TFileSystem}.BaseDirectory" />
		public IDirectoryInfo BaseDirectory
			=> FileSystem.DirectoryInfo.New(_basePath);

		/// <inheritdoc cref="IFileSystemInitializer{TFileSystem}.FileSystem" />
		public TFileSystem FileSystem { get; }

		/// <inheritdoc cref="IFileSystemInitializer{TFileSystem}.this[int]" />
		public IFileSystemInfo this[int index]
			=> _initializedFileSystemInfos[index];

		/// <inheritdoc cref="IFileSystemInitializer{TFileSystem}.WithAFile(string?)" />
		public IFileSystemFileInitializer<TFileSystem> WithAFile(string? extension = null)
		{
			IRandom random = (FileSystem as MockFileSystem)?
			   .RandomSystem.Random.Shared ?? RandomFactory.Shared;
			string fileName;
			do
			{
				fileName =
					$"{random.GenerateFileName()}-{random.Next(10000)}.{random.GenerateFileExtension(extension)}";
			} while (FileSystem.File.Exists(
				FileSystem.Path.Combine(_basePath, fileName)));

			return WithFile(fileName);
		}

		/// <inheritdoc cref="IFileSystemInitializer{TFileSystem}.WithASubdirectory()" />
		public IFileSystemDirectoryInitializer<TFileSystem> WithASubdirectory()
		{
			IRandom random = (FileSystem as MockFileSystem)?
			   .RandomSystem.Random.Shared ?? RandomFactory.Shared;
			string directoryName;
			do
			{
				directoryName =
					$"{random.GenerateFileName()}-{random.Next(10000)}";
			} while (FileSystem.Directory.Exists(
				FileSystem.Path.Combine(_basePath, directoryName)));

			return WithSubdirectory(directoryName);
		}

		/// <inheritdoc cref="IFileSystemInitializer{TFileSystem}.WithFile(string)" />
		public IFileSystemFileInitializer<TFileSystem> WithFile(string fileName)
		{
			IFileInfo fileInfo = FileSystem.FileInfo.New(
				FileSystem.Path.Combine(_basePath, fileName));
			if (fileInfo.Exists)
			{
				throw new TestingException(
					$"The file '{fileInfo.FullName}' already exists!");
			}

			FileSystem.File.WriteAllText(fileInfo.FullName, null);
			_initializedFileSystemInfos.Add(
				_initializedFileSystemInfos.Count,
				fileInfo);

			fileInfo.Refresh();
			return new FileInitializer<TFileSystem>(this, fileInfo);
		}

		/// <inheritdoc cref="IFileSystemInitializer{TFileSystem}.WithSubdirectory(string)" />
		public IFileSystemDirectoryInitializer<TFileSystem> WithSubdirectory(
			string directoryName)
		{
			IDirectoryInfo directoryInfo = FileSystem.DirectoryInfo.New(
				FileSystem.Path.Combine(_basePath, directoryName));
			if (directoryInfo.Exists)
			{
				throw new TestingException(
					$"The directory '{directoryInfo.FullName}' already exists!");
			}

			FileSystem.Directory.CreateDirectory(directoryInfo.FullName);
			_initializedFileSystemInfos.Add(
				_initializedFileSystemInfos.Count,
				directoryInfo);

			directoryInfo.Refresh();
			return new DirectoryInitializer<TFileSystem>(this, directoryInfo);
		}

		#endregion
	}
}