using System.IO;

namespace Testably.Abstractions.Tests.Internal;

public class FileInfoWrapperTests
{
	[Fact]
	public void FromFileInfo_Null_ShouldReturnNull()
	{
		RealFileSystem fileSystem = new();

		FileInfoWrapper? result = FileInfoWrapper
			.FromFileInfo(null, fileSystem);

		result.Should().BeNull();
	}

	[Theory]
	[AutoData]
	public void FromFileInfo_ShouldBePartOfFileSystem(string path)
	{
		RealFileSystem fileSystem = new();

		FileInfoWrapper result = FileInfoWrapper
			.FromFileInfo(new FileInfo(path), fileSystem);

		result.Directory.Should().NotBeNull();
	}
}
