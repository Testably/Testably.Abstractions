namespace Testably.Abstractions.Tests.FileSystem.Directory;

public partial class Tests
{
	[Theory]
	[AutoData]
	public async Task AdjustTimes_WhenCreatingAFile_ShouldAdjustTimes(
		string path1, string path2, string fileName)
	{
		SkipIfLongRunningTestsShouldBeSkipped();

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
			await That(parentCreationTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
			await That(parentLastAccessTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());
		}
		else
		{
			await That(parentLastAccessTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
		}

		await That(parentLastWriteTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());

		DateTime rootCreationTime = FileSystem.Directory.GetCreationTimeUtc(path1);
		DateTime rootLastAccessTime = FileSystem.Directory.GetLastAccessTimeUtc(path1);
		DateTime rootLastWriteTime = FileSystem.Directory.GetLastWriteTimeUtc(path1);

		await That(rootCreationTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
		await That(rootLastAccessTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
		await That(rootLastWriteTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
	}

#if FEATURE_FILESYSTEM_LINK
	[Theory]
	[AutoData]
	public async Task AdjustTimes_WhenCreatingASymbolicLink_ShouldAdjustTimes(
		string path1, string path2, string fileName, string pathToTarget)
	{
		SkipIfLongRunningTestsShouldBeSkipped();

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
			await That(parentCreationTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
			await That(parentLastAccessTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());
		}
		else
		{
			await That(parentLastAccessTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
		}

		await That(parentLastWriteTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());

		DateTime rootCreationTime = FileSystem.Directory.GetCreationTimeUtc(path1);
		DateTime rootLastAccessTime = FileSystem.Directory.GetLastAccessTimeUtc(path1);
		DateTime rootLastWriteTime = FileSystem.Directory.GetLastWriteTimeUtc(path1);

		await That(rootCreationTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
		await That(rootLastAccessTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
		await That(rootLastWriteTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
	}
#endif

	[Theory]
	[AutoData]
	public async Task AdjustTimes_WhenDeletingAFile_ShouldAdjustTimes(
		string path1, string path2, string fileName)
	{
		SkipIfLongRunningTestsShouldBeSkipped();

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
			await That(parentCreationTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
			await That(parentLastAccessTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());
		}
		else
		{
			await That(parentLastAccessTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
		}

		await That(parentLastWriteTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());

		DateTime rootCreationTime = FileSystem.Directory.GetCreationTimeUtc(path1);
		DateTime rootLastAccessTime = FileSystem.Directory.GetLastAccessTimeUtc(path1);
		DateTime rootLastWriteTime = FileSystem.Directory.GetLastWriteTimeUtc(path1);

		await That(rootCreationTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
		await That(rootLastAccessTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
		await That(rootLastWriteTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
	}

	[Theory]
	[AutoData]
	public async Task AdjustTimes_WhenUpdatingAFile_ShouldAdjustTimesOnlyOnWindows(
		string path1, string path2, string fileName)
	{
		Skip.If(Test.IsNetFramework && FileSystem is RealFileSystem,
			"Works unreliable on .NET Framework");
		SkipIfLongRunningTestsShouldBeSkipped();

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

		await That(parentCreationTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
		if (Test.RunsOnWindows)
		{
			await That(parentLastAccessTime).IsOnOrAfter(updateTimeStart.ApplySystemClockTolerance());
		}
		else
		{
			await That(parentLastAccessTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
		}

		await That(parentLastWriteTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
	}
}
