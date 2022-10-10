using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Testably.Abstractions;

public sealed partial class FileSystem
{
	private sealed class DirectoryInfoFactory : IFileSystem.IDirectoryInfoFactory
	{
		internal DirectoryInfoFactory(FileSystem fileSystem)
		{
			FileSystem = fileSystem;
		}

		#region IDirectoryInfoFactory Members

		/// <inheritdoc cref="IFileSystem.IFileSystemExtensionPoint.FileSystem" />
		public IFileSystem FileSystem { get; }

		/// <inheritdoc cref="IFileSystem.IDirectoryInfoFactory.New" />
		public IFileSystem.IDirectoryInfo New(string path)
			=> DirectoryInfoWrapper.FromDirectoryInfo(
				new DirectoryInfo(path),
				FileSystem);

		/// <inheritdoc cref="IFileSystem.IDirectoryInfoFactory.Wrap(DirectoryInfo)" />
		[return: NotNullIfNotNull("directoryInfo")]
		public IFileSystem.IDirectoryInfo? Wrap(DirectoryInfo? directoryInfo)
			=> DirectoryInfoWrapper.FromDirectoryInfo(
				directoryInfo,
				FileSystem);

		#endregion
	}
}