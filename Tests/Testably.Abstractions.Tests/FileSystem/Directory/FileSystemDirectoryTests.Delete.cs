using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.Directory;

public abstract partial class FileSystemDirectoryTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void Delete_FullPath_ShouldDeleteDirectory(string directoryName)
	{
		IFileSystem.IDirectoryInfo result =
			FileSystem.Directory.CreateDirectory(directoryName);

		FileSystem.Directory.Delete(result.FullName);

		FileSystem.Directory.Exists(directoryName).Should().BeFalse();
		result.Exists.Should().BeFalse();
	}

	[SkippableTheory]
	[AutoData]
	public void Delete_MissingDirectory_ShouldDeleteDirectory(string directoryName)
	{
		string expectedPath = System.IO.Path.Combine(BasePath, directoryName);
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.Delete(directoryName);
		});

		exception.Should().BeOfType<DirectoryNotFoundException>()
		   .Which.Message.Should()
		   .Be($"Could not find a part of the path '{expectedPath}'.");
	}

	[SkippableTheory]
	[AutoData]
	public void Delete_Recursive_MissingDirectory_ShouldDeleteDirectory(
		string directoryName)
	{
		string expectedPath = System.IO.Path.Combine(BasePath, directoryName);
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.Delete(directoryName, true);
		});

		exception.Should().BeOfType<DirectoryNotFoundException>()
		   .Which.Message.Should()
		   .Be($"Could not find a part of the path '{expectedPath}'.");
	}

	[SkippableTheory]
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

	[SkippableTheory]
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

	[SkippableTheory]
	[AutoData]
	public void Delete_ShouldAdjustTimes(string path, string subdirectoryName)
	{
		Test.SkipIfLongRunningTestsShouldBeSkipped(FileSystem);

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
			   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
			   .BeOnOrBefore(creationTimeEnd);
			lastAccessTime.Should()
			   .BeOnOrAfter(updateTime.ApplySystemClockTolerance());
		}
		else
		{
			lastAccessTime.Should()
			   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
			   .BeOnOrBefore(creationTimeEnd);
		}

		lastWriteTime.Should()
		   .BeOnOrAfter(updateTime.ApplySystemClockTolerance());
	}

	[SkippableTheory]
	[AutoData]
	public void Delete_ShouldDeleteDirectory(string directoryName)
	{
		IFileSystem.IDirectoryInfo result =
			FileSystem.Directory.CreateDirectory(directoryName);

		FileSystem.Directory.Delete(directoryName);

		bool exists = FileSystem.Directory.Exists(directoryName);

		exists.Should().BeFalse();
		result.Exists.Should().BeFalse();
	}

	[SkippableTheory]
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

	[SkippableTheory]
	[AutoData]
	public void Delete_WithSubdirectory_ShouldNotDeleteDirectory(
		string path, string subdirectory)
	{
		FileSystem.Directory.CreateDirectory(FileSystem.Path.Combine(path, subdirectory));
		FileSystem.Directory.Exists(path).Should().BeTrue();

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.Delete(path);
		});

		exception.Should().BeOfType<IOException>();
#if !NETFRAMEWORK
		if (Test.RunsOnWindows)
		{
			// Path information only included in exception message on Windows and not in .NET Framework
			exception.Should().BeOfType<IOException>()
			   .Which.Message.Should().Contain($"'{System.IO.Path.Combine(BasePath, path)}'");
		}
#endif
	}
}