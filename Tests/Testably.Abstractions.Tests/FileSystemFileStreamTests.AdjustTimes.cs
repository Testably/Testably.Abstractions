namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemFileStreamTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	[FileSystemTests.FileStream("AdjustTimes")]
	public void ReadByte_ShouldAdjustTimes(string path)
	{
		Test.SkipIfLongRunningTestsShouldBeSkipped(FileSystem);

		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.File.WriteAllText(path, "foo");
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		TimeSystem.Thread.Sleep(1500);
		DateTime updateTime = TimeSystem.DateTime.UtcNow;
		using FileSystemStream stream = FileSystem.File.OpenRead(path);

		stream.ReadByte();

		DateTime creationTime = FileSystem.File.GetCreationTimeUtc(path);
		DateTime lastAccessTime = FileSystem.File.GetLastAccessTimeUtc(path);
		DateTime lastWriteTime = FileSystem.File.GetLastWriteTimeUtc(path);

		creationTime.Should()
		   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
		   .BeOnOrBefore(creationTimeEnd);
		lastAccessTime.Should()
		   .BeOnOrAfter(updateTime);
		lastWriteTime.Should()
		   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
		   .BeOnOrBefore(creationTimeEnd);
	}
	[SkippableTheory]
	[AutoData]
	[FileSystemTests.FileStream("AdjustTimes")]
	public void Read_ShouldAdjustTimes(string path)
	{
		Test.SkipIfLongRunningTestsShouldBeSkipped(FileSystem);

		byte[] buffer = new byte[2];
		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.File.WriteAllText(path, "foo");
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		TimeSystem.Thread.Sleep(1500);
		DateTime updateTime = TimeSystem.DateTime.UtcNow;
		using FileSystemStream stream = FileSystem.File.OpenRead(path);

		_ = stream.Read(buffer, 0, 2);

		DateTime creationTime = FileSystem.File.GetCreationTimeUtc(path);
		DateTime lastAccessTime = FileSystem.File.GetLastAccessTimeUtc(path);
		DateTime lastWriteTime = FileSystem.File.GetLastWriteTimeUtc(path);

		creationTime.Should()
		   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
		   .BeOnOrBefore(creationTimeEnd);
		lastAccessTime.Should()
		   .BeOnOrAfter(updateTime);
		lastWriteTime.Should()
		   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
		   .BeOnOrBefore(creationTimeEnd);
	}
}