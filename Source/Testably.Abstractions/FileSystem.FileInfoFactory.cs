using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Testably.Abstractions;

public sealed partial class FileSystem
{
	private sealed class FileInfoFactory : IFileInfoFactory
	{
		internal FileInfoFactory(FileSystem fileSystem)
		{
			FileSystem = fileSystem;
		}

		#region IFileInfoFactory Members

		/// <inheritdoc cref="IFileSystemExtensionPoint.FileSystem" />
		public IFileSystem FileSystem { get; }

		/// <inheritdoc cref="IFileInfoFactory.New" />
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
}