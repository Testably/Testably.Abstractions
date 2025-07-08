using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.DirectoryInfo;

[FileSystemTests]
public partial class AttributesTests
{
	[Theory]
	[AutoData]
	public async Task Attributes_ClearAllAttributes_ShouldRemainDirectory(string path)
	{
		FileSystem.Directory.CreateDirectory(path);
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New(path);

		sut.Attributes = 0;

		FileSystem.Directory.Exists(path).Should().BeTrue();
		FileSystem.File.Exists(path).Should().BeFalse();
		await That(sut.Attributes).HasFlag(FileAttributes.Directory);
	}

	[Theory]
	[InlineAutoData(FileAttributes.ReadOnly)]
	[InlineAutoData(FileAttributes.Normal)]
	public async Task Attributes_WhenFileIsExisting_SetterShouldChangeAttributesOnFileSystem(
		FileAttributes attributes, string path)
	{
		FileSystem.Directory.CreateDirectory(path);
		IDirectoryInfo sut1 = FileSystem.DirectoryInfo.New(path);
		IDirectoryInfo sut2 = FileSystem.DirectoryInfo.New(path);

		sut1.Attributes = attributes;
		FileAttributes expectedAttributes = sut1.Attributes;

		await That(sut2.Attributes).IsEqualTo(expectedAttributes);
	}

	[Fact]
	public void Attributes_WhenFileIsMissing_SetterShouldThrowFileNotFoundException()
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New("missing file");

		Exception? exception = Record.Exception(() =>
		{
			sut.Attributes = FileAttributes.Normal;
		});

		exception.Should().BeException<FileNotFoundException>(hResult: -2147024894);
	}

	[Fact]
	public async Task Attributes_WhenFileIsMissing_ShouldReturnMinusOne()
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New("missing file");
		FileAttributes expected = (FileAttributes)(-1);

		await That(sut.Attributes).IsEqualTo(expected);
	}
}
