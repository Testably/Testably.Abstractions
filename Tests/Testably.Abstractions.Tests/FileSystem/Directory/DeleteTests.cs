using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.Directory;

[FileSystemTests]
public partial class DeleteTests
{
	[Theory]
	[AutoData]
	public async Task Delete_CaseDifferentPath_ShouldThrowDirectoryNotFoundException_OnLinux(
		string directoryName)
	{
		directoryName = directoryName.ToLowerInvariant();
		FileSystem.Directory.CreateDirectory(directoryName.ToUpperInvariant());
		string expectedPath = FileSystem.Path.Combine(BasePath, directoryName);

		void Act()
		{
			FileSystem.Directory.Delete(directoryName);
		}

		if (Test.RunsOnLinux)
		{
			await That(Act).Throws<DirectoryNotFoundException>()
				.WithMessageContaining($"'{expectedPath}'").And
				.WithHResult(-2147024893);
		}
		else
		{
			await That(Act).DoesNotThrow();
			await That(FileSystem.Directory.Exists(directoryName.ToUpperInvariant())).IsFalse();
		}
	}

	[Theory]
	[AutoData]
	public async Task Delete_FullPath_ShouldDeleteDirectory(string directoryName)
	{
		IDirectoryInfo result =
			FileSystem.Directory.CreateDirectory(directoryName);

		FileSystem.Directory.Delete(result.FullName);

		await That(FileSystem.Directory.Exists(directoryName)).IsFalse();
		await That(result.Exists).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task Delete_MissingDirectory_ShouldThrowDirectoryNotFoundException(
		string directoryName)
	{
		string expectedPath = FileSystem.Path.Combine(BasePath, directoryName);

		void Act()
		{
			FileSystem.Directory.Delete(directoryName);
		}

		await That(Act).Throws<DirectoryNotFoundException>()
			.WithMessageContaining($"'{expectedPath}'").And
			.WithHResult(-2147024893);
	}

	[Theory]
	[AutoData]
	public async Task Delete_ReadonlyDirectory_ShouldThrowIOExceptionOnWindows(string path)
	{
		IDirectoryInfo sut = FileSystem.Directory.CreateDirectory(path);
		sut.Attributes = FileAttributes.ReadOnly;
		sut.Refresh();

		void Act()
		{
			FileSystem.Directory.Delete(path);
		}

		await That(Act).Throws<IOException>()
			.OnlyIf(Test.RunsOnWindows)
			.WithMessage(Test.IsNetFramework
				? $"Access to the path '{path}' is denied."
				: $"Access to the path '{sut.FullName}' is denied.").And
			.WithHResult(-2146232800);
	}

	[Theory]
	[AutoData]
	public async Task Delete_Recursive_MissingDirectory_ShouldThrowDirectoryNotFoundException(
		string directoryName)
	{
		string expectedPath = FileSystem.Path.Combine(BasePath, directoryName);

		void Act()
		{
			FileSystem.Directory.Delete(directoryName, true);
		}

		await That(Act).Throws<DirectoryNotFoundException>()
			.WithMessageContaining($"'{expectedPath}'").And
			.WithHResult(-2147024893);
	}

	[Theory]
	[AutoData]
	public async Task Delete_Recursive_WithFileInSubdirectory_ShouldDeleteDirectoryWithContent(
		string path, string subdirectory, string fileName, string fileContent)
	{
		FileSystem.Directory.CreateDirectory(path);
		string filePath = FileSystem.Path.Combine(path, fileName);
		FileSystem.File.WriteAllText(filePath, fileContent);

		string subdirectoryPath = FileSystem.Path.Combine(path, subdirectory);
		FileSystem.Directory.CreateDirectory(subdirectoryPath);
		string subdirectoryFilePath = FileSystem.Path.Combine(path, subdirectory, fileName);
		FileSystem.File.WriteAllText(subdirectoryFilePath, fileContent);

		await That(FileSystem.Directory.Exists(path)).IsTrue();

		FileSystem.Directory.Delete(path, true);

		await That(FileSystem.Directory.Exists(path)).IsFalse();
		await That(FileSystem.File.Exists(filePath)).IsFalse();
		await That(FileSystem.Directory.Exists(subdirectoryPath)).IsFalse();
		await That(FileSystem.File.Exists(subdirectoryFilePath)).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task Delete_Recursive_WithOpenFile_ShouldThrowIOException_OnWindows(
		string path, string filename)
	{
		FileSystem.Initialize()
			.WithSubdirectory(path);
		string filePath = FileSystem.Path.Combine(path, filename);
		FileSystemStream openFile = FileSystem.File.OpenWrite(filePath);
		openFile.Write([0], 0, 1);
		openFile.Flush();

		void Act()
		{
			FileSystem.Directory.Delete(path, true);
			openFile.Write([0], 0, 1);
			openFile.Flush();
		}

		if (Test.RunsOnWindows)
		{
			await That(Act).Throws<IOException>()
				.WithMessageContaining($"{filename}'").And
				.WithHResult(-2147024864);
			await That(FileSystem.File.Exists(filePath)).IsTrue();
		}
		else
		{
			await That(Act).DoesNotThrow();
			await That(FileSystem.File.Exists(filePath)).IsFalse();
		}
	}

	[Theory]
	[AutoData]
	public async Task
		Delete_Recursive_WithSimilarNamedFile_ShouldOnlyDeleteDirectoryAndItsContents(
			string subdirectory)
	{
		string fileName = $"{subdirectory}.txt";
		FileSystem.Initialize()
			.WithSubdirectory(subdirectory).Initialized(s => s
				.WithAFile()
				.WithASubdirectory())
			.WithFile(fileName);

		FileSystem.Directory.Delete(subdirectory, true);

		await That(FileSystem.Directory.Exists(subdirectory)).IsFalse();
		await That(FileSystem.File.Exists(fileName)).IsTrue();
	}

	[Theory]
	[AutoData]
	public async Task Delete_Recursive_WithSubdirectory_ShouldDeleteDirectoryWithContent(
		string path, string subdirectory)
	{
		string subdirectoryPath = FileSystem.Path.Combine(path, subdirectory);
		FileSystem.Directory.CreateDirectory(subdirectoryPath);
		await That(FileSystem.Directory.Exists(path)).IsTrue();

		FileSystem.Directory.Delete(path, true);

		await That(FileSystem.Directory.Exists(path)).IsFalse();
		await That(FileSystem.Directory.Exists(subdirectoryPath)).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task Delete_ShouldAdjustTimes(string path, string subdirectoryName)
	{
		SkipIfLongRunningTestsShouldBeSkipped();

		string subdirectoryPath = FileSystem.Path.Combine(path, subdirectoryName);
		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.Directory.CreateDirectory(subdirectoryPath);
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		TimeSystem.Thread.Sleep(FileTestHelper.AdjustTimesDelay);
		DateTime updateTime = TimeSystem.DateTime.UtcNow;

		FileSystem.Directory.Delete(subdirectoryPath);

		DateTime creationTime = FileSystem.Directory.GetCreationTimeUtc(path);
		DateTime lastAccessTime = FileSystem.Directory.GetLastAccessTimeUtc(path);
		DateTime lastWriteTime = FileSystem.Directory.GetLastWriteTimeUtc(path);

		if (Test.RunsOnWindows)
		{
			await That(creationTime).IsBetween(creationTimeStart).And(creationTimeEnd)
				.Within(TimeComparison.Tolerance);
			await That(lastAccessTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());
		}
		else
		{
			await That(lastAccessTime).IsBetween(creationTimeStart).And(creationTimeEnd)
				.Within(TimeComparison.Tolerance);
		}

		await That(lastWriteTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());
	}

	[Theory]
	[AutoData]
	public async Task Delete_ShouldDeleteDirectory(string directoryName)
	{
		IDirectoryInfo result =
			FileSystem.Directory.CreateDirectory(directoryName);

		FileSystem.Directory.Delete(directoryName);

		await That(FileSystem.Directory.Exists(directoryName)).IsFalse();
		await That(result.Exists).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task Delete_WhenFile_ShouldThrowIOException(
		string directoryName)
	{
		FileSystem.File.WriteAllText(directoryName, "");
		string expectedPath = FileSystem.Path.Combine(BasePath, directoryName);

		void Act()
		{
			FileSystem.Directory.Delete(directoryName, true);
		}

		if (Test.IsNetFramework)
		{
			await That(Act).Throws<IOException>()
				.WithMessage("*The directory name is invalid*").AsWildcard();
		}
		else if (Test.RunsOnWindows)
		{
			await That(Act).Throws<IOException>()
				.WithMessage($"*The directory name is invalid*{expectedPath}*").AsWildcard().And
				.WithHResult(-2147024629);
		}
		else
		{
			await That(Act).Throws<DirectoryNotFoundException>()
				.WithMessage($"*Could not find a part of the path*{expectedPath}*").AsWildcard().And
				.WithHResult(-2147024893);
		}
	}

	[Theory]
	[AutoData]
	public async Task Delete_WithSimilarNamedFile_ShouldOnlyDeleteDirectory(
		string subdirectory)
	{
		string fileName = $"{subdirectory}.txt";
		FileSystem.Initialize()
			.WithSubdirectory(subdirectory)
			.WithFile(fileName);

		FileSystem.Directory.Delete(subdirectory);

		await That(FileSystem.Directory.Exists(subdirectory)).IsFalse();
		await That(FileSystem.File.Exists(fileName)).IsTrue();
	}

	[Theory]
	[AutoData]
	public async Task Delete_WithSubdirectory_ShouldThrowIOException_AndNotDeleteDirectory(
		string path, string subdirectory)
	{
		FileSystem.Directory.CreateDirectory(FileSystem.Path.Combine(path, subdirectory));
		await That(FileSystem.Directory.Exists(path)).IsTrue();

		void Act()
		{
			FileSystem.Directory.Delete(path);
		}

		await That(Act).Throws<IOException>()
			.WithHResult(Test.DependsOnOS(windows: -2147024751, macOS: 66, linux: 39)).And
			.WithMessageContaining(!Test.RunsOnWindows || Test.IsNetFramework
				? null
				: $"'{FileSystem.Path.Combine(BasePath, path)}'");
		await That(FileSystem.Directory.Exists(path)).IsTrue();
	}

	[Theory]
	[AutoData]
	[InlineData(null)]
	public async Task Delete_CurrentDirectory_ShouldThrowIOException_OnWindows(string? nested)
	{
		// Arrange
		string directory = FileSystem.Directory.GetCurrentDirectory();
		string expectedExceptionDirectory = directory;

		if (nested != null)
		{
			string nestedDirectory = FileSystem.Path.Combine(directory, nested);
			FileSystem.Directory.CreateDirectory(nestedDirectory);
			FileSystem.Directory.SetCurrentDirectory(nestedDirectory);
			expectedExceptionDirectory = nestedDirectory;
		}

		// Act
		void Act()
		{
			FileSystem.Directory.Delete(directory, true);
		}

		try
		{
			// Assert
			await That(Act).ThrowsExactly<IOException>().OnlyIf(Test.RunsOnWindows).Which
				.HasMessage(
					$"The process cannot access the file '*{expectedExceptionDirectory}' because it is being used by another process."
				).AsWildcard();
		}
		finally
		{
			if (Test.RunsOnWindows)
			{
				// Cleanup
				FileSystem.Directory.SetCurrentDirectory(BasePath);
			}
		}
	}

	[Theory]
	[InlineData("next")]
	[InlineData("next", "sub")]
	public async Task Delete_DirNextToCurrentDirectory_ShouldNotThrow(params string[] paths)
	{
		// Arrange
		string directory = FileSystem.Directory.GetCurrentDirectory();
		// Intended missing separator, we want to test that the handle does not affect similar paths
		string nextTo = directory + FileSystem.Path.Combine(paths);

		FileSystem.Directory.CreateDirectory(nextTo);
		FileSystem.Directory.SetCurrentDirectory(nextTo);

		// Act
		void Act()
		{
			FileSystem.Directory.Delete(directory, true);
		}

		// Assert
		await That(Act).DoesNotThrow();

		await That(FileSystem.Directory.Exists(directory)).IsFalse();
		await That(FileSystem.Directory.Exists(nextTo)).IsTrue();
	}
}
