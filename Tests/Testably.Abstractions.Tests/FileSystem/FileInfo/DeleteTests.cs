using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.FileInfo;

[FileSystemTests]
public partial class DeleteTests
{
	[Theory]
	[AutoData]
	public void Delete_MissingDirectory_ShouldThrowDirectoryNotFoundException(
		string missingDirectory, string fileName)
	{
		string filePath = FileSystem.Path.Combine(missingDirectory, fileName);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.FileInfo.New(filePath).Delete();
		});

		exception.Should().BeException<DirectoryNotFoundException>(hResult: -2147024893);
	}

	[Theory]
	[AutoData]
	public void Delete_MissingFile_ShouldDoNothing(
		string fileName)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.FileInfo.New(fileName).Delete();
		});

		exception.Should().BeNull();
	}

	[Theory]
	[AutoData]
	public void Delete_ShouldRefreshExistsCache_ExceptOnNetFramework(string path)
	{
		FileSystem.File.WriteAllText(path, "some content");
		IFileInfo sut = FileSystem.FileInfo.New(path);
		sut.Should().Exist();

		sut.Delete();

		if (Test.IsNetFramework)
		{
			sut.Should().Exist();
		}
		else
		{
			sut.Should().NotExist();
		}

		FileSystem.Should().NotHaveFile(path);
	}

	[Theory]
	[AutoData]
	public void Delete_WithOpenFile_ShouldThrowIOException_OnWindows(string filename)
	{
		FileSystem.Initialize();
		FileSystemStream openFile = FileSystem.File.OpenWrite(filename);
		openFile.Write([0], 0, 1);
		openFile.Flush();
		IFileInfo sut = FileSystem.FileInfo.New(filename);
		Exception? exception = Record.Exception(() =>
		{
			sut.Delete();
			openFile.Write([0], 0, 1);
			openFile.Flush();
		});

		if (Test.RunsOnWindows)
		{
			exception.Should().BeException<IOException>(
				messageContains: $"{filename}'",
				hResult: -2147024864);
			FileSystem.Should().HaveFile(filename);
		}
		else
		{
			exception.Should().BeNull();
			FileSystem.Should().NotHaveFile(filename);
		}
	}
}
