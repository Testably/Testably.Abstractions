#if !NETFRAMEWORK
#endif

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemDirectoryTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	[FileSystemTests.Directory("AdjustTimes")]
	public void AdjustTimes_WhenCreatingAFile_ShouldAdjustTimes(
		string path1, string path2, string fileName)
	{
		//Test.SkipIfLongRunningTestsShouldBeSkipped(FileSystem);

		string subdirectoryPath = FileSystem.Path.Combine(path1, path2);
		string filePath = FileSystem.Path.Combine(subdirectoryPath, fileName);
		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.Directory.CreateDirectory(subdirectoryPath);
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		TimeSystem.Thread.Sleep(FileTestHelper.AdjustTimesDelay);
		DateTime updateTime = TimeSystem.DateTime.UtcNow;

		FileSystem.File.WriteAllText(filePath, null);

		DateTime parentCreationTime =
			FileSystem.Directory.GetCreationTimeUtc(subdirectoryPath);
		DateTime parentLastAccessTime =
			FileSystem.Directory.GetLastAccessTimeUtc(subdirectoryPath);
		DateTime parentLastWriteTime =
			FileSystem.Directory.GetLastWriteTimeUtc(subdirectoryPath);

		if (Test.RunsOnWindows)
		{
			parentCreationTime.Should()
			   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
			   .BeOnOrBefore(creationTimeEnd);
			parentLastAccessTime.Should()
			   .BeOnOrAfter(updateTime.ApplySystemClockTolerance());
		}
		else
		{
			parentLastAccessTime.Should()
			   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
			   .BeOnOrBefore(creationTimeEnd);
		}

		parentLastWriteTime.Should()
		   .BeOnOrAfter(updateTime.ApplySystemClockTolerance());

		DateTime rootCreationTime = FileSystem.Directory.GetCreationTimeUtc(path1);
		DateTime rootLastAccessTime = FileSystem.Directory.GetLastAccessTimeUtc(path1);
		DateTime rootLastWriteTime = FileSystem.Directory.GetLastWriteTimeUtc(path1);

		rootCreationTime.Should()
		   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
		   .BeOnOrBefore(creationTimeEnd);
		rootLastAccessTime.Should()
		   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
		   .BeOnOrBefore(creationTimeEnd);
		rootLastWriteTime.Should()
		   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
		   .BeOnOrBefore(creationTimeEnd);
	}
}