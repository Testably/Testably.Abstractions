using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

[FileSystemTests]
public partial class AttributesTests
{
	[Theory]
	[AutoData]
	public void Attributes_SetDirectoryAttribute_ShouldRemainFile(string path)
	{
		FileSystem.File.WriteAllText(path, "foo");
		IFileInfo sut = FileSystem.FileInfo.New(path);

		sut.Attributes = FileAttributes.Directory;

		sut.Attributes.Should().NotHaveFlag(FileAttributes.Directory);
	}

	[Theory]
	[AutoData]
	public void Attributes_ShouldNotHaveDirectoryAttribute(string path)
	{
		FileSystem.File.WriteAllText(path, "foo");
		IFileInfo sut = FileSystem.FileInfo.New(path);

		sut.Attributes.Should().NotHaveFlag(FileAttributes.Directory);
	}
}
