using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class DeleteTests
{
	[Theory]
	[AutoData]
	public async Task Delete_MissingDirectory_ShouldThrowDirectoryNotFoundException(
		string missingDirectory, string fileName)
	{
		string filePath = FileSystem.Path.Combine(missingDirectory, fileName);

		void Act()
		{
			FileSystem.File.Delete(filePath);
		}

		await That(Act).Throws<DirectoryNotFoundException>().WithHResult(-2147024893);
	}

	[Theory]
	[AutoData]
	public async Task Delete_MissingFile_ShouldDoNothing(
		string fileName)
	{
		void Act()
		{
			FileSystem.File.Delete(fileName);
		}

		await That(Act).DoesNotThrow();
	}

	[Theory]
	[AutoData]
	public async Task Delete_WhenDirectory_ShouldThrowUnauthorizedAccessException(
		string fileName)
	{
		FileSystem.Directory.CreateDirectory(fileName);
		string expectedPath = FileSystem.Path.Combine(BasePath, fileName);
		void Act()
		{
			FileSystem.File.Delete(fileName);
		}

		await That(Act).Throws<UnauthorizedAccessException>()
			.WithMessageContaining($"'{expectedPath}'").And
			.WithHResult(-2147024891);
	}

	[Theory]
	[AutoData]
	public async Task Delete_WithOpenFile_ShouldThrowIOException_OnWindows(string filename)
	{
		FileSystem.Initialize();
		FileSystemStream openFile = FileSystem.File.OpenWrite(filename);
		openFile.Write([0], 0, 1);
		openFile.Flush();
		void Act()
		{
			FileSystem.File.Delete(filename);
			openFile.Write([0], 0, 1);
			openFile.Flush();
		}

		if (Test.RunsOnWindows)
		{
			await That(Act).Throws<IOException>()
				.WithMessageContaining($"{filename}'").And
				.WithHResult(-2147024864);
			await That(FileSystem.File.Exists(filename)).IsTrue();
		}
		else
		{
			await That(Act).DoesNotThrow();
			await That(FileSystem.File.Exists(filename)).IsFalse();
		}
	}
}
