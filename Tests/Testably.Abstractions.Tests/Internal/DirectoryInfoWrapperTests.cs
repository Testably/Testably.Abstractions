using System.IO;

namespace Testably.Abstractions.Tests.Internal;

public class DirectoryInfoWrapperTests
{
	[Fact]
	public void FromDirectoryInfo_Null_ShouldReturnNull()
	{
		RealFileSystem fileSystem = new();

		DirectoryInfoWrapper? result = DirectoryInfoWrapper
			.FromDirectoryInfo(null, fileSystem);

		result.Should().BeNull();
	}

	[Theory]
	[AutoData]
	public void FromDirectoryInfo_ShouldUseFileSystem(string path)
	{
		RealFileSystem fileSystem = new();

		DirectoryInfoWrapper result = DirectoryInfoWrapper
			.FromDirectoryInfo(new DirectoryInfo(path), fileSystem);

		result.Root.Should().NotBeNull();
	}
}
