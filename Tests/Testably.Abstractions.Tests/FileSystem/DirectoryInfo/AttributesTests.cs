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

		await That(FileSystem.Directory.Exists(path)).IsTrue();
		await That(FileSystem.File.Exists(path)).IsFalse();
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
	public async Task Attributes_WhenFileIsMissing_SetterShouldThrowFileNotFoundException()
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New("missing file");

		void Act()
		{
			sut.Attributes = FileAttributes.Normal;
		}

		await That(Act).Throws<FileNotFoundException>().WithHResult(-2147024894);
	}

	[Fact]
	public async Task Attributes_WhenFileIsMissing_ShouldReturnMinusOne()
	{
		IDirectoryInfo sut = FileSystem.DirectoryInfo.New("missing file");
		FileAttributes expected = (FileAttributes)(-1);

		await That(sut.Attributes).IsEqualTo(expected);
	}
}
