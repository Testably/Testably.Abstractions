using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.FileSystem;

internal sealed class DirectoryInfoFactoryMock : IDirectoryInfoFactory
{
	private readonly MockFileSystem _fileSystem;

	internal DirectoryInfoFactoryMock(MockFileSystem fileSystem)
	{
		_fileSystem = fileSystem;
	}

	#region IDirectoryInfoFactory Members

	/// <inheritdoc cref="IFileSystemEntity.FileSystem" />
	public IFileSystem FileSystem
		=> _fileSystem;

	/// <inheritdoc cref="IDirectoryInfoFactory.New(string)" />
	public IDirectoryInfo New(string path)
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.DirectoryInfo.RegisterMethod(nameof(New),
				path);

		return DirectoryInfoMock.New(
			_fileSystem.Storage.GetLocation(path
				.EnsureValidArgument(_fileSystem, nameof(path))),
			_fileSystem);
	}

	/// <inheritdoc cref="IDirectoryInfoFactory.Wrap(DirectoryInfo)" />
	[return: NotNullIfNotNull("directoryInfo")]
	public IDirectoryInfo? Wrap(DirectoryInfo? directoryInfo)
	{
		using IDisposable registration = _fileSystem.StatisticsRegistration
			.DirectoryInfo.RegisterMethod(nameof(Wrap),
				directoryInfo);

		if (_fileSystem.SimulationMode != SimulationMode.Native)
		{
			throw new NotSupportedException(
				"Wrapping a DirectoryInfo in a simulated file system is not supported!");
		}

		return DirectoryInfoMock.New(
			_fileSystem.Storage.GetLocation(
				directoryInfo?.FullName,
				directoryInfo?.ToString()),
			_fileSystem);
	}

	#endregion
}
