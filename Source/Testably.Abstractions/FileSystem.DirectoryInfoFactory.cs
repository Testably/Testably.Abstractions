using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Testably.Abstractions;

public sealed partial class FileSystem
{
	private sealed class DirectoryInfoFactory : IDirectoryInfoFactory
	{
		internal DirectoryInfoFactory(FileSystem fileSystem)
		{
			FileSystem = fileSystem;
		}

		#region IDirectoryInfoFactory Members

		/// <inheritdoc cref="IFileSystemExtensionPoint.FileSystem" />
		public IFileSystem FileSystem { get; }

		/// <inheritdoc cref="IDirectoryInfoFactory.New" />
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
}