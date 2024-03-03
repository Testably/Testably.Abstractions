using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.FileSystem;

internal sealed class FileInfoFactoryMock : IFileInfoFactory
{
	private readonly MockFileSystem _fileSystem;

	internal FileInfoFactoryMock(MockFileSystem fileSystem)
	{
		_fileSystem = fileSystem;
	}

	#region IFileInfoFactory Members

	/// <inheritdoc cref="IFileSystemEntity.FileSystem" />
	public IFileSystem FileSystem
		=> _fileSystem;

	/// <inheritdoc cref="IFileInfoFactory.FromFileName(string)" />
	[Obsolete("Use `IFileInfoFactory.New(string)` instead")]
	public IFileInfo FromFileName(string fileName)
	{
		using IDisposable registration = Register(nameof(FromFileName),
			fileName);

		return New(fileName);
	}

	/// <inheritdoc cref="IFileInfoFactory.New(string)" />
	public IFileInfo New(string fileName)
	{
		using IDisposable registration = Register(nameof(New),
			fileName);

		if (fileName == null)
		{
			throw new ArgumentNullException(nameof(fileName));
		}

		if (fileName.IsEffectivelyEmpty(_fileSystem))
		{
			throw ExceptionFactory.PathIsEmpty("path");
		}

		return FileInfoMock.New(
			_fileSystem.Storage.GetLocation(fileName),
			_fileSystem);
	}

	/// <inheritdoc cref="IFileInfoFactory.Wrap(FileInfo)" />
	[return: NotNullIfNotNull("fileInfo")]
	public IFileInfo? Wrap(FileInfo? fileInfo)
	{
		using IDisposable registration = Register(nameof(Wrap),
			fileInfo);

		return FileInfoMock.New(
			_fileSystem.Storage.GetLocation(
				fileInfo?.FullName,
				fileInfo?.ToString()),
			_fileSystem);
	}

	#endregion

	private IDisposable Register(string name, params object?[] parameters)
		=> _fileSystem.FileSystemStatistics.FileInfoStatistics.Register(name, parameters);
}
