using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.Testing.Statistics;

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

	/// <inheritdoc cref="IDirectoryInfoFactory.FromDirectoryName(string)" />
	[Obsolete("Use `IDirectoryInfoFactory.New(string)` instead")]
	public IDirectoryInfo FromDirectoryName(string directoryName)
	{
		using IDisposable registration = Register(nameof(FromDirectoryName),
			directoryName);

		return New(directoryName);
	}

	/// <inheritdoc cref="IDirectoryInfoFactory.New(string)" />
	public IDirectoryInfo New(string path)
	{
		using IDisposable registration = Register(nameof(New),
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
		using IDisposable registration = Register(nameof(Wrap),
			directoryInfo);

		return DirectoryInfoMock.New(
			_fileSystem.Storage.GetLocation(
				directoryInfo?.FullName,
				directoryInfo?.ToString()),
			_fileSystem);
	}

	#endregion

	private IDisposable Register<T1>(string name, T1 parameter1)
		=> _fileSystem.StatisticsRegistration.DirectoryInfo.RegisterMethod(name, ParameterDescription.FromParameter(parameter1));
}
