using System;
using System.Collections.Generic;
using System.Linq;
using Testably.Abstractions.RandomSystem;
using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.Initializer;

internal class FileSystemInitializer<TFileSystem>
	: IFileSystemInitializer<TFileSystem>
	where TFileSystem : IFileSystem
{
	private readonly string _basePath;

	private readonly Dictionary<int, IFileSystemInfo>
		_initializedFileSystemInfos = new();

	public FileSystemInitializer(TFileSystem fileSystem, string basePath)
	{
		_basePath = basePath;
		FileSystem = fileSystem;
	}

	protected FileSystemInitializer(FileSystemInitializer<TFileSystem> parent)
	{
		FileSystem = parent.FileSystem;
		_initializedFileSystemInfos = parent._initializedFileSystemInfos;
		_basePath = parent._basePath;
	}

	internal FileSystemInitializer(FileSystemInitializer<TFileSystem> parent,
		IDirectoryInfo subdirectory)
	{
		FileSystem = parent.FileSystem;
		using IDisposable release = FileSystem.IgnoreStatistics();
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

	/// <inheritdoc cref="IFileSystemInitializer{TFileSystem}.With{TDescription}(TDescription[])" />
	public IFileSystemInitializer<TFileSystem> With<TDescription>(TDescription[] descriptions)
		where TDescription : FileSystemInfoDescription
		=> With(descriptions.Cast<FileSystemInfoDescription>().ToArray());

	/// <inheritdoc cref="IFileSystemInitializer{TFileSystem}.With(FileSystemInfoDescription[])" />
	public IFileSystemInitializer<TFileSystem> With(
		params FileSystemInfoDescription[] descriptions)
	{
		foreach (FileSystemInfoDescription description in descriptions)
		{
			WithFileOrDirectory(description);
		}

		return this;
	}

	/// <inheritdoc cref="IFileSystemInitializer{TFileSystem}.WithAFile(string?)" />
	public IFileSystemFileInitializer<TFileSystem> WithAFile(string? extension = null)
	{
		using IDisposable release = FileSystem.IgnoreStatistics();
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
		using IDisposable release = FileSystem.IgnoreStatistics();
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
		IFileInfo fileInfo = WithFile(new FileDescription(fileName));
		return new FileInitializer<TFileSystem>(this, fileInfo);
	}

	/// <inheritdoc cref="IFileSystemInitializer{TFileSystem}.WithSubdirectories(string[])" />
	public IFileSystemInitializer<TFileSystem> WithSubdirectories(params string[] paths)
	{
		foreach (string directory in paths)
		{
			WithSubdirectory(directory);
		}

		return this;
	}

	/// <inheritdoc cref="IFileSystemInitializer{TFileSystem}.WithSubdirectory(string)" />
	public IFileSystemDirectoryInitializer<TFileSystem> WithSubdirectory(
		string directoryName)
	{
		IDirectoryInfo directoryInfo = WithDirectory(new DirectoryDescription(directoryName));
		return new DirectoryInitializer<TFileSystem>(this, directoryInfo);
	}

	#endregion

	private IDirectoryInfo WithDirectory(DirectoryDescription directory)
	{
		using IDisposable release = FileSystem.IgnoreStatistics();
		IDirectoryInfo directoryInfo = FileSystem.DirectoryInfo.New(
			FileSystem.Path.Combine(_basePath, directory.Name));
		if (directoryInfo.Exists)
		{
			throw new TestingException(
				$"The directory '{directoryInfo.FullName}' already exists!");
		}

		if (FileSystem.File.Exists(directoryInfo.FullName))
		{
			throw new TestingException(
				$"A file '{directoryInfo.FullName}' already exists!");
		}

		FileSystem.Directory.CreateDirectory(directoryInfo.FullName);

		if (directory.Children.Length > 0)
		{
			FileSystemInitializer<TFileSystem> subdirectoryInitializer = new(this, directoryInfo);
			foreach (FileSystemInfoDescription children in directory.Children)
			{
				subdirectoryInitializer.WithFileOrDirectory(children);
			}
		}

		_initializedFileSystemInfos.Add(
			_initializedFileSystemInfos.Count,
			directoryInfo);

		directoryInfo.Refresh();
		return directoryInfo;
	}

	private IFileInfo WithFile(FileDescription file)
	{
		using IDisposable release = FileSystem.IgnoreStatistics();
		IFileInfo fileInfo = FileSystem.FileInfo.New(
			FileSystem.Path.Combine(_basePath, file.Name));
		if (fileInfo.Exists)
		{
			throw new TestingException(
				$"The file '{fileInfo.FullName}' already exists!");
		}

		if (FileSystem.Directory.Exists(fileInfo.FullName))
		{
			throw new TestingException(
				$"A directory '{fileInfo.FullName}' already exists!");
		}

		if (fileInfo.Directory != null)
		{
			FileSystem.Directory.CreateDirectory(fileInfo.Directory.FullName);
		}

		if (file.Bytes != null)
		{
			FileSystem.File.WriteAllBytes(fileInfo.FullName, file.Bytes);
		}
		else
		{
			FileSystem.File.WriteAllText(fileInfo.FullName, file.Content);
		}

		_initializedFileSystemInfos.Add(
			_initializedFileSystemInfos.Count,
			fileInfo);

		fileInfo.Refresh();

		fileInfo.IsReadOnly = file.IsReadOnly;
		return fileInfo;
	}

	private void WithFileOrDirectory(FileSystemInfoDescription description)
	{
		if (description is FileDescription file)
		{
			WithFile(file);
		}

		if (description is DirectoryDescription directory)
		{
			WithDirectory(directory);
		}
	}
}
