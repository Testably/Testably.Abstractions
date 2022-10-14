using System.IO;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemFileTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	[FileSystemTests.File(nameof(IFileSystem.IFile.SetAttributes))]
	public void SetAttributes_GetAttributesShouldReturnAttributes(
		string path, FileAttributes attributes)
	{
		FileSystem.File.WriteAllText(path, null);

		FileSystem.File.SetAttributes(path, attributes);

		FileAttributes result = FileSystem.File.GetAttributes(path);
		result.Should().Be(attributes);
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.File(nameof(IFileSystem.IFile.SetAttributes))]
	public void SetAttributes_MissingFile_GetAttributesShouldReturnAttributes(
		string path, FileAttributes attributes)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.SetAttributes(path, attributes);
		});

		exception.Should().BeOfType<FileNotFoundException>()
		   .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.File(nameof(IFileSystem.IFile.SetAttributes))]
	public void SetAttributes_ShouldNotAdjustTimes(string path, FileAttributes attributes)
	{
		Test.SkipIfLongRunningTestsShouldBeSkipped(FileSystem);

		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.File.WriteAllText(path, null);
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		TimeSystem.Thread.Sleep(FileTestHelper.AdjustTimesDelay);

		FileSystem.File.SetAttributes(path, attributes);
		
		DateTime creationTime = FileSystem.File.GetCreationTimeUtc(path);
		DateTime lastAccessTime = FileSystem.File.GetLastAccessTimeUtc(path);
		DateTime lastWriteTime = FileSystem.File.GetLastWriteTimeUtc(path);

		if (Test.RunsOnWindows)
		{
			creationTime.Should()
			   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
			   .BeOnOrBefore(creationTimeEnd);
		}
		lastAccessTime.Should()
		   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
		   .BeOnOrBefore(creationTimeEnd);
		lastWriteTime.Should()
		   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
		   .BeOnOrBefore(creationTimeEnd);
	}
}