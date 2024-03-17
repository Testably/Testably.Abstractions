using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Testably.Abstractions.FileSystem;

internal sealed class DirectoryInfoFactory : IDirectoryInfoFactory
{
	internal DirectoryInfoFactory(RealFileSystem fileSystem)
	{
		FileSystem = fileSystem;
	}

	#region IDirectoryInfoFactory Members

	/// <inheritdoc cref="IFileSystemEntity.FileSystem" />
	public IFileSystem FileSystem { get; }

	/// <inheritdoc cref="IDirectoryInfoFactory.FromDirectoryName(string)" />
	[Obsolete("Use `IDirectoryInfoFactory.New(string)` instead")]
	[ExcludeFromCodeCoverage]
	public IDirectoryInfo FromDirectoryName(string directoryName)
		=> New(directoryName);

	/// <inheritdoc cref="IDirectoryInfoFactory.New(string)" />
	public IDirectoryInfo New(string path)
		=> DirectoryInfoWrapper.FromDirectoryInfo(
			new DirectoryInfo(path),
			FileSystem);

	/// <inheritdoc cref="IDirectoryInfoFactory.Wrap(DirectoryInfo)" />
	[return: NotNullIfNotNull("directoryInfo")]
	public IDirectoryInfo? Wrap(DirectoryInfo? directoryInfo)
		=> DirectoryInfoWrapper.FromDirectoryInfo(
			directoryInfo,
			FileSystem);

	#endregion
}
