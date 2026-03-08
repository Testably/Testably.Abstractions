using System.IO;

namespace Testably.Abstractions.Tests.Internal;

public class FileSystemInfoWrapperTests
{
	[Test]
	public async Task FromFileSystemInfo_Null_ShouldReturnNull()
	{
		RealFileSystem fileSystem = new();

		FileSystemInfoWrapper? result = FileSystemInfoWrapper
			.FromFileSystemInfo(null, fileSystem);

		await That(result).IsNull();
	}

	[Test]
	[Arguments("my-path")]
	public async Task FromFileSystemInfo_WithDirectoryInfo_ShouldReturnDirectoryInfoWrapper(
		string path)
	{
		RealFileSystem fileSystem = new();

		FileSystemInfoWrapper result = FileSystemInfoWrapper
			.FromFileSystemInfo(new DirectoryInfo(path), fileSystem);

		await That(result).IsExactly<DirectoryInfoWrapper>();
	}

	[Test]
	[Arguments("my-path")]
	public async Task FromFileSystemInfo_WithFileInfo_ShouldReturnFileInfoWrapper(string path)
	{
		RealFileSystem fileSystem = new();

		FileSystemInfoWrapper result = FileSystemInfoWrapper
			.FromFileSystemInfo(new FileInfo(path), fileSystem);

		await That(result).IsExactly<FileInfoWrapper>();
	}
}
