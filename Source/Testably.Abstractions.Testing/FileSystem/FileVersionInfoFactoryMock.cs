using System;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing.FileSystem;

internal sealed class FileVersionInfoFactoryMock
	: IFileVersionInfoFactory
{
	private readonly MockFileSystem _fileSystem;

	internal FileVersionInfoFactoryMock(MockFileSystem fileSystem)
	{
		_fileSystem = fileSystem;
	}

	#region IFileVersionInfoFactory Members

	/// <inheritdoc cref="IFileSystemEntity.FileSystem" />
	public IFileSystem FileSystem
		=> _fileSystem;

	/// <inheritdoc cref="IFileVersionInfoFactory.GetVersionInfo(string)" />
	public IFileVersionInfo GetVersionInfo(string fileName)
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.FileVersionInfo.RegisterMethod(nameof(GetVersionInfo), fileName);

		fileName.EnsureValidFormat(_fileSystem, nameof(fileName));
		IStorageLocation location = _fileSystem.Storage.GetLocation(fileName);
		location.ThrowExceptionIfNotFound(_fileSystem);

		FileVersionInfoContainer container = _fileSystem.Storage.GetVersionInfo(location)
		                                     ?? FileVersionInfoContainer.None;

		return FileVersionInfoMock.New(_fileSystem.Storage.GetLocation(fileName), container,
			_fileSystem);
	}

	#endregion
}
