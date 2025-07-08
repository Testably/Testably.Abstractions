using System.IO;

namespace Testably.Abstractions.Tests.Internal;

public class FileInfoWrapperTests
{
	[Fact]
	public async Task FromFileInfo_Null_ShouldReturnNull()
	{
		RealFileSystem fileSystem = new();

		FileInfoWrapper? result = FileInfoWrapper
			.FromFileInfo(null, fileSystem);

		await That(result).IsNull();
	}

	[Theory]
	[AutoData]
	public async Task FromFileInfo_ShouldBePartOfFileSystem(string path)
	{
		RealFileSystem fileSystem = new();

		FileInfoWrapper result = FileInfoWrapper
			.FromFileInfo(new FileInfo(path), fileSystem);

		await That(result.Directory).IsNotNull();
	}
}
