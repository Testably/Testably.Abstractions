namespace Testably.Abstractions.Tests.FileSystem.Directory;

public abstract partial class Tests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void AdjustTimes_WhenCreatingAFile_ShouldAdjustTimes(
		string path1, string path2, string fileName)
	{
		Test.SkipIfLongRunningTestsShouldBeSkipped(FileSystem);

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

#if FEATURE_FILESYSTEM_LINK
	[SkippableTheory]
	[AutoData]
	public void AdjustTimes_WhenCreatingASymbolicLink_ShouldAdjustTimes(
		string path1, string path2, string fileName, string pathToTarget)
	{
		Test.SkipIfLongRunningTestsShouldBeSkipped(FileSystem);

		string subdirectoryPath = FileSystem.Path.Combine(path1, path2);
		string filePath = FileSystem.Path.Combine(subdirectoryPath, fileName);
		FileSystem.File.WriteAllText(pathToTarget, null);

		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.Directory.CreateDirectory(subdirectoryPath);
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		TimeSystem.Thread.Sleep(FileTestHelper.AdjustTimesDelay);
		DateTime updateTime = TimeSystem.DateTime.UtcNow;

		FileSystem.File.CreateSymbolicLink(filePath, pathToTarget);

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
#endif

	[SkippableTheory]
	[AutoData]
	public void AdjustTimes_WhenDeletingAFile_ShouldAdjustTimes(
		string path1, string path2, string fileName)
	{
		Test.SkipIfLongRunningTestsShouldBeSkipped(FileSystem);

		string subdirectoryPath = FileSystem.Path.Combine(path1, path2);
		string filePath = FileSystem.Path.Combine(subdirectoryPath, fileName);
		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.Directory.CreateDirectory(subdirectoryPath);
		FileSystem.File.WriteAllText(filePath, null);
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		TimeSystem.Thread.Sleep(FileTestHelper.AdjustTimesDelay);
		DateTime updateTime = TimeSystem.DateTime.UtcNow;

		FileSystem.File.Delete(filePath);

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

	[SkippableTheory]
	[AutoData]
	public void AdjustTimes_WhenUpdatingAFile_ShouldAdjustTimesOnlyOnWindows(
		string path1, string path2, string fileName)
	{
		Skip.If(Test.IsNetFramework && FileSystem is RealFileSystem,
			"Works unreliable on .NET Framework");
		Test.SkipIfLongRunningTestsShouldBeSkipped(FileSystem);

		string subdirectoryPath = FileSystem.Path.Combine(path1, path2);
		string filePath = FileSystem.Path.Combine(subdirectoryPath, fileName);
		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.Directory.CreateDirectory(subdirectoryPath);
		FileSystem.File.WriteAllText(filePath, "");
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		TimeSystem.Thread.Sleep(FileTestHelper.AdjustTimesDelay);
		DateTime updateTimeStart = TimeSystem.DateTime.UtcNow;

		FileSystem.File.AppendAllText(filePath, "foo");

		DateTime parentCreationTime =
			FileSystem.Directory.GetCreationTimeUtc(subdirectoryPath);
		DateTime parentLastAccessTime =
			FileSystem.Directory.GetLastAccessTimeUtc(subdirectoryPath);
		DateTime parentLastWriteTime =
			FileSystem.Directory.GetLastWriteTimeUtc(subdirectoryPath);

		parentCreationTime.Should()
			.BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
			.BeOnOrBefore(creationTimeEnd);
		if (Test.RunsOnWindows)
		{
			parentLastAccessTime.Should()
				.BeOnOrAfter(updateTimeStart.ApplySystemClockTolerance());
		}
		else
		{
			parentLastAccessTime.Should()
				.BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
				.BeOnOrBefore(creationTimeEnd);
		}

		parentLastWriteTime.Should()
			.BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
			.BeOnOrBefore(creationTimeEnd);
	}
}
