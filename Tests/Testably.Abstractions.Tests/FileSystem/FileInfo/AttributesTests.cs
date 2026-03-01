using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

[FileSystemTests]
public class AttributesTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	[AutoArguments]
	public async Task Attributes_SetDirectoryAttribute_ShouldRemainFile(string path)
	{
		FileSystem.File.WriteAllText(path, "foo");
		IFileInfo sut = FileSystem.FileInfo.New(path);

		sut.Attributes = FileAttributes.Directory;

		await That(sut.Attributes).DoesNotHaveFlag(FileAttributes.Directory);
	}

	[Test]
	[AutoArguments]
	public async Task Attributes_ShouldNotHaveDirectoryAttribute(string path)
	{
		FileSystem.File.WriteAllText(path, "foo");
		IFileInfo sut = FileSystem.FileInfo.New(path);

		await That(sut.Attributes).DoesNotHaveFlag(FileAttributes.Directory);
	}
}
