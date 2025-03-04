using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.Directory;

[FileSystemTests]
public partial class DeleteTests
{
	[Theory]
	[AutoData]
	public void
		Delete_CaseDifferentPath_ShouldThrowDirectoryNotFoundException_OnLinux(
			string directoryName)
	{
		directoryName = directoryName.ToLowerInvariant();
		FileSystem.Directory.CreateDirectory(directoryName.ToUpperInvariant());
		string expectedPath = FileSystem.Path.Combine(BasePath, directoryName);
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.Delete(directoryName);
		});

		if (Test.RunsOnLinux)
		{
			exception.Should().BeException<DirectoryNotFoundException>($"'{expectedPath}'",
				hResult: -2147024893);
		}
		else
		{
			exception.Should().BeNull();
			FileSystem.Directory.Exists(directoryName.ToUpperInvariant()).Should().BeFalse();
		}
	}

	[Theory]
	[AutoData]
	public void Delete_FullPath_ShouldDeleteDirectory(string directoryName)
	{
		IDirectoryInfo result =
			FileSystem.Directory.CreateDirectory(directoryName);

		FileSystem.Directory.Delete(result.FullName);

		FileSystem.Directory.Exists(directoryName).Should().BeFalse();
		result.Exists.Should().BeFalse();
	}

	[Theory]
	[AutoData]
	public void Delete_MissingDirectory_ShouldThrowDirectoryNotFoundException(
		string directoryName)
	{
		string expectedPath = FileSystem.Path.Combine(BasePath, directoryName);
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.Delete(directoryName);
		});

		exception.Should().BeException<DirectoryNotFoundException>($"'{expectedPath}'",
			hResult: -2147024893);
	}

	[Theory]
	[AutoData]
	public void Delete_Recursive_MissingDirectory_ShouldThrowDirectoryNotFoundException(
		string directoryName)
	{
		string expectedPath = FileSystem.Path.Combine(BasePath, directoryName);
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.Delete(directoryName, true);
		});

		exception.Should().BeException<DirectoryNotFoundException>($"'{expectedPath}'",
			hResult: -2147024893);
	}

	[Theory]
	[AutoData]
	public void Delete_Recursive_WithFileInSubdirectory_ShouldDeleteDirectoryWithContent(
		string path, string subdirectory, string fileName, string fileContent)
	{
		FileSystem.Directory.CreateDirectory(path);
		string filePath = FileSystem.Path.Combine(path, fileName);
		FileSystem.File.WriteAllText(filePath, fileContent);

		string subdirectoryPath = FileSystem.Path.Combine(path, subdirectory);
		FileSystem.Directory.CreateDirectory(subdirectoryPath);
		string subdirectoryFilePath = FileSystem.Path.Combine(path, subdirectory, fileName);
		FileSystem.File.WriteAllText(subdirectoryFilePath, fileContent);

		FileSystem.Directory.Exists(path).Should().BeTrue();

		FileSystem.Directory.Delete(path, true);

		FileSystem.Directory.Exists(path).Should().BeFalse();
		FileSystem.File.Exists(filePath).Should().BeFalse();
		FileSystem.Directory.Exists(subdirectoryPath).Should().BeFalse();
		FileSystem.File.Exists(subdirectoryFilePath).Should().BeFalse();
	}

	[Theory]
	[AutoData]
	public void Delete_Recursive_WithOpenFile_ShouldThrowIOException_OnWindows(
		string path, string filename)
	{
		FileSystem.Initialize()
			.WithSubdirectory(path);
		string filePath = FileSystem.Path.Combine(path, filename);
		FileSystemStream openFile = FileSystem.File.OpenWrite(filePath);
		openFile.Write([0], 0, 1);
		openFile.Flush();
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.Delete(path, true);
			openFile.Write([0], 0, 1);
			openFile.Flush();
		});

		if (Test.RunsOnWindows)
		{
			exception.Should().BeException<IOException>($"{filename}'",
				hResult: -2147024864);
			FileSystem.File.Exists(filePath).Should().BeTrue();
		}
		else
		{
			exception.Should().BeNull();
			FileSystem.File.Exists(filePath).Should().BeFalse();
		}
	}

	[Theory]
	[AutoData]
	public void
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

		FileSystem.Directory.Exists(subdirectory).Should().BeFalse();
		FileSystem.File.Exists(fileName).Should().BeTrue();
	}

	[Theory]
	[AutoData]
	public void Delete_Recursive_WithSubdirectory_ShouldDeleteDirectoryWithContent(
		string path, string subdirectory)
	{
		string subdirectoryPath = FileSystem.Path.Combine(path, subdirectory);
		FileSystem.Directory.CreateDirectory(subdirectoryPath);
		FileSystem.Directory.Exists(path).Should().BeTrue();

		FileSystem.Directory.Delete(path, true);

		FileSystem.Directory.Exists(path).Should().BeFalse();
		FileSystem.Directory.Exists(subdirectoryPath).Should().BeFalse();
	}

	[Theory]
	[AutoData]
	public void Delete_ShouldAdjustTimes(string path, string subdirectoryName)
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
			creationTime.Should()
				.BeBetween(creationTimeStart, creationTimeEnd);
			lastAccessTime.Should()
				.BeOnOrAfter(updateTime.ApplySystemClockTolerance());
		}
		else
		{
			lastAccessTime.Should()
				.BeBetween(creationTimeStart, creationTimeEnd);
		}

		lastWriteTime.Should()
			.BeOnOrAfter(updateTime.ApplySystemClockTolerance());
	}

	[Theory]
	[AutoData]
	public void Delete_ShouldDeleteDirectory(string directoryName)
	{
		IDirectoryInfo result =
			FileSystem.Directory.CreateDirectory(directoryName);

		FileSystem.Directory.Delete(directoryName);

		FileSystem.Directory.Exists(directoryName).Should().BeFalse();
		result.Exists.Should().BeFalse();
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
	public void Delete_WithSimilarNamedFile_ShouldOnlyDeleteDirectory(
		string subdirectory)
	{
		string fileName = $"{subdirectory}.txt";
		FileSystem.Initialize()
			.WithSubdirectory(subdirectory)
			.WithFile(fileName);

		FileSystem.Directory.Delete(subdirectory);

		FileSystem.Directory.Exists(subdirectory).Should().BeFalse();
		FileSystem.File.Exists(fileName).Should().BeTrue();
	}

	[Theory]
	[AutoData]
	public void Delete_WithSubdirectory_ShouldThrowIOException_AndNotDeleteDirectory(
		string path, string subdirectory)
	{
		FileSystem.Directory.CreateDirectory(FileSystem.Path.Combine(path, subdirectory));
		FileSystem.Directory.Exists(path).Should().BeTrue();

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.Delete(path);
		});

		exception.Should().BeException<IOException>(
			hResult: Test.DependsOnOS(windows: -2147024751, macOS: 66, linux: 39),
			// Path information only included in exception message on Windows and not in .NET Framework
			messageContains: !Test.RunsOnWindows || Test.IsNetFramework
				? null
				: $"'{FileSystem.Path.Combine(BasePath, path)}'");
		FileSystem.Directory.Exists(path).Should().BeTrue();
	}
}
