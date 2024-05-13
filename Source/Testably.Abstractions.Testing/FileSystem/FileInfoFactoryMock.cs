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

	/// <inheritdoc cref="IFileInfoFactory.New(string)" />
	public IFileInfo New(string fileName)
	{
		using IDisposable registration = RegisterMethod(nameof(New),
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
		using IDisposable registration = RegisterMethod(nameof(Wrap),
			fileInfo);

		if (_fileSystem.SimulationMode != SimulationMode.Native)
		{
			throw new NotSupportedException(
				"Wrapping a FileInfo in a simulated file system is not supported!");
		}

		return FileInfoMock.New(
			_fileSystem.Storage.GetLocation(
				fileInfo?.FullName,
				fileInfo?.ToString()),
			_fileSystem);
	}

	#endregion

	private IDisposable RegisterMethod<T1>(string name, T1 parameter1)
		=> _fileSystem.StatisticsRegistration.FileInfo.RegisterMethod(name,
			parameter1);
}
