using System;

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

		return FileVersionInfoMock.New(_fileSystem.Storage.GetLocation(fileName), _fileSystem);
	}

	#endregion
}
