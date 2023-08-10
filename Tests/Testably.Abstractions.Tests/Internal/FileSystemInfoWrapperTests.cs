using System.IO;
using DirectoryInfoWrapper = Testably.Abstractions.FileSystem.DirectoryInfoWrapper;
using FileInfoWrapper = Testably.Abstractions.FileSystem.FileInfoWrapper;

namespace Testably.Abstractions.Tests.Internal;

public class FileSystemInfoWrapperTests
{
	[Fact]
	public void FromFileSystemInfo_Null_ShouldReturnNull()
	{
		RealFileSystem fileSystem = new();

		FileSystemInfoWrapper? result = FileSystemInfoWrapper
			.FromFileSystemInfo(null, fileSystem);

		result.Should().BeNull();
	}

	[Theory]
	[AutoData]
	public void FromFileSystemInfo_WithDirectoryInfo_ShouldReturnDirectoryInfoWrapper(string path)
	{
		RealFileSystem fileSystem = new();

		FileSystemInfoWrapper result = FileSystemInfoWrapper
			.FromFileSystemInfo(new DirectoryInfo(path), fileSystem);

		result.Should().BeOfType<DirectoryInfoWrapper>();
	}

	[Theory]
	[AutoData]
	public void FromFileSystemInfo_WithFileInfo_ShouldReturnFileInfoWrapper(string path)
	{
		RealFileSystem fileSystem = new();

		FileSystemInfoWrapper result = FileSystemInfoWrapper
			.FromFileSystemInfo(new FileInfo(path), fileSystem);

		result.Should().BeOfType<FileInfoWrapper>();
	}
}
