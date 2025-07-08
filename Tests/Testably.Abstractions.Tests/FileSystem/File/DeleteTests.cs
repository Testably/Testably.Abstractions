using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.File;

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
			FileSystem.File.Delete(filePath);
		});

		exception.Should().BeException<DirectoryNotFoundException>(hResult: -2147024893);
	}

	[Theory]
	[AutoData]
	public async Task Delete_MissingFile_ShouldDoNothing(
		string fileName)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.Delete(fileName);
		});

		await That(exception).IsNull();
	}

	[Theory]
	[AutoData]
	public void Delete_WhenDirectory_ShouldThrowUnauthorizedAccessException(
		string fileName)
	{
		FileSystem.Directory.CreateDirectory(fileName);
		string expectedPath = FileSystem.Path.Combine(BasePath, fileName);
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.Delete(fileName);
		});

		exception.Should().BeException<UnauthorizedAccessException>($"'{expectedPath}'",
			hResult: -2147024891);
	}

	[Theory]
	[AutoData]
	public async Task Delete_WithOpenFile_ShouldThrowIOException_OnWindows(string filename)
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
			FileSystem.File.Exists(filename).Should().BeTrue();
		}
		else
		{
			await That(exception).IsNull();
			FileSystem.File.Exists(filename).Should().BeFalse();
		}
	}
}
