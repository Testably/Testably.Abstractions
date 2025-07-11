using System.IO;

namespace Testably.Abstractions.Tests.Internal;

public class DirectoryInfoWrapperTests
{
	[Fact]
	public async Task FromDirectoryInfo_Null_ShouldReturnNull()
	{
		RealFileSystem fileSystem = new();

		DirectoryInfoWrapper? result = DirectoryInfoWrapper
			.FromDirectoryInfo(null, fileSystem);

		await That(result).IsNull();
	}

	[Theory]
	[AutoData]
	public async Task FromDirectoryInfo_ShouldBePartOfFileSystem(string path)
	{
		RealFileSystem fileSystem = new();

		DirectoryInfoWrapper result = DirectoryInfoWrapper
			.FromDirectoryInfo(new DirectoryInfo(path), fileSystem);

		await That(result.Root).IsNotNull();
	}
}
