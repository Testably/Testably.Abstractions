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

	/// <inheritdoc cref="IFileSystemExtensionPoint.FileSystem" />
	public IFileSystem FileSystem { get; }

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
