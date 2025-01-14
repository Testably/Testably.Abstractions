using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfo;

[FileSystemTests]
public partial class AttributesTests
{
	[SkippableTheory]
	[AutoData]
	public void Attributes_ClearAllAttributes_ShouldRemainDirectory(string path)
	{
		FileSystem.Directory.CreateDirectory(path);
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);

		sut.Attributes = 0;

		FileSystem.Should().HaveDirectory(path);
		FileSystem.Should().NotHaveFile(path);
		sut.Attributes.Should().HaveFlag(FileAttributes.Directory);
	}

	[SkippableTheory]
	[InlineAutoData(FileAttributes.ReadOnly)]
	[InlineAutoData(FileAttributes.Normal)]
	public void Attributes_WhenFileIsExisting_SetterShouldChangeAttributesOnFileSystem(
		FileAttributes attributes, string path)
	{
		FileSystem.Directory.CreateDirectory(path);
		IDirectoryInfo sut1 = FileSystem.DirectoryInfo.New(path);
		IDirectoryInfo sut2 = FileSystem.DirectoryInfo.New(path);

		sut1.Attributes = attributes;
		FileAttributes expectedAttributes = sut1.Attributes;

		sut2.Attributes.Should().Be(expectedAttributes);
	}

	[SkippableFact]
	public void Attributes_WhenFileIsMissing_SetterShouldThrowFileNotFoundException()
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New("missing file");

		Exception? exception = Record.Exception(() =>
		{
			sut.Attributes = FileAttributes.Normal;
		});

		exception.Should().BeException<FileNotFoundException>(hResult: -2147024894);
	}

	[SkippableFact]
	public void Attributes_WhenFileIsMissing_ShouldReturnMinusOne()
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New("missing file");
		FileAttributes expected = (FileAttributes)(-1);

		sut.Attributes.Should().Be(expected);
	}
}
