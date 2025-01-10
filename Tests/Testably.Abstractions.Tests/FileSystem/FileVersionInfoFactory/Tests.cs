using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileVersionInfoFactory;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class Tests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void GetVersionInfo_ArbitraryFile_ShouldHaveFileNameSet(string fileName)
	{
		string filePath = FileSystem.Path.GetFullPath(fileName);
		FileSystem.File.WriteAllText(fileName, "foo");

		IFileVersionInfo result = FileSystem.FileVersionInfo.GetVersionInfo(fileName);

		result.FileName.Should().Be(filePath);
	}

	[SkippableTheory]
	[AutoData]
	public void GetVersionInfo_MissingFile_ShouldThrowFileNotFoundException(
		string path)
	{
		Exception? exception = Record.Exception(() =>
			FileSystem.FileVersionInfo.GetVersionInfo(path));

		exception.Should().BeException<FileNotFoundException>(
			FileSystem.Path.GetFullPath(path),
			hResult: -2147024894);
	}
}
