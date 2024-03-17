using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Testably.Abstractions.FileSystem;

internal sealed class FileInfoFactory : IFileInfoFactory
{
	internal FileInfoFactory(RealFileSystem fileSystem)
	{
		FileSystem = fileSystem;
	}

	#region IFileInfoFactory Members

	/// <inheritdoc cref="IFileSystemEntity.FileSystem" />
	public IFileSystem FileSystem { get; }

	/// <inheritdoc cref="IFileInfoFactory.FromFileName(string)" />
	[Obsolete("Use `IFileInfoFactory.New(string)` instead")]
	[ExcludeFromCodeCoverage]
	public IFileInfo FromFileName(string fileName)
		=> New(fileName);

	/// <inheritdoc cref="IFileInfoFactory.New(string)" />
	public IFileInfo New(string fileName)
		=> FileInfoWrapper.FromFileInfo(
			new FileInfo(fileName),
			FileSystem);

	/// <inheritdoc cref="IFileInfoFactory.Wrap(FileInfo)" />
	[return: NotNullIfNotNull("fileInfo")]
	public IFileInfo? Wrap(FileInfo? fileInfo)
		=> FileInfoWrapper.FromFileInfo(
			fileInfo,
			FileSystem);

	#endregion
}
