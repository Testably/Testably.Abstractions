using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Testably.Abstractions.FileSystem;

internal sealed class FileVersionInfoFactory : IFileVersionInfoFactory
{
	internal FileVersionInfoFactory(RealFileSystem fileSystem)
	{
		FileSystem = fileSystem;
	}

	#region IFileSystemWatcherFactory Members

	/// <inheritdoc cref="IFileSystemEntity.FileSystem" />
	public IFileSystem FileSystem { get; }

	/// <inheritdoc cref="IFileVersionInfoFactory.GetVersionInfo(string)" />
	public IFileVersionInfo GetVersionInfo(string fileName)
		=> FileVersionInfoWrapper.FromFileVersionInfo(
			FileVersionInfo.GetVersionInfo(fileName),
			FileSystem);

	#endregion
}
