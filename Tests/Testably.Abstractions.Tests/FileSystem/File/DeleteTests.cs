using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class DeleteTests
{
	[SkippableTheory]
	[AutoData]
	public void Delete_MissingDirectory_ShouldThrowDirectoryNotFoundException(
		string missingDirectory, string fileName)
	{
		string filePath = FileSystem.Path.Combine(missingDirectory, fileName);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.Delete(filePath);
		});

		exception.Should().BeException<DirectoryNotFoundException>(hResult: -2147024893);
	}

	[SkippableTheory]
	[AutoData]
	public void Delete_MissingFile_ShouldDoNothing(
		string fileName)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.Delete(fileName);
		});

		exception.Should().BeNull();
	}

	[SkippableTheory]
	[AutoData]
	public void Delete_WithOpenFile_ShouldThrowIOException_OnWindows(string filename)
	{
		FileSystem.Initialize();
		FileSystemStream openFile = FileSystem.File.OpenWrite(filename);
		openFile.Write([0], 0, 1);
		openFile.Flush();
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.Delete(filename);
			openFile.Write([0], 0, 1);
			openFile.Flush();
		});

		if (Test.RunsOnWindows)
		{
			exception.Should().BeException<IOException>($"{filename}'",
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
