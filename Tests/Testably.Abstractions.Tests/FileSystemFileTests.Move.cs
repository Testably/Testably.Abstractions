using System.IO;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemFileTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	[FileSystemTests.File(nameof(IFileSystem.IFile.Move))]
	public void Move_DestinationExists_ShouldThrowIOExceptionAndNotMoveFile(
		string sourceName,
		string destinationName,
		string sourceContents,
		string destinationContents)
	{
		FileSystem.File.WriteAllText(sourceName, sourceContents);
		FileSystem.File.WriteAllText(destinationName, destinationContents);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.Move(sourceName, destinationName);
		});

		exception.Should().BeOfType<IOException>();
		FileSystem.File.Exists(sourceName).Should().BeTrue();
		FileSystem.File.ReadAllText(sourceName).Should().Be(sourceContents);
		FileSystem.File.Exists(destinationName).Should().BeTrue();
		FileSystem.File.ReadAllText(destinationName).Should().Be(destinationContents);
	}

#if FEATURE_FILE_MOVETO_OVERWRITE
	[SkippableTheory]
	[AutoData]
	[FileSystemTests.File(nameof(IFileSystem.IFile.Move))]
	public void Move_DestinationExists_WithOverwrite_ShouldOverwriteDestination(
		string sourceName,
		string destinationName,
		string sourceContents,
		string destinationContents)
	{
		FileSystem.File.WriteAllText(sourceName, sourceContents);
		FileSystem.File.WriteAllText(destinationName, destinationContents);

		FileSystem.File.Move(sourceName, destinationName, true);

		FileSystem.File.Exists(sourceName).Should().BeFalse();
		FileSystem.File.Exists(destinationName).Should().BeTrue();
		FileSystem.File.ReadAllText(destinationName).Should().Be(sourceContents);
	}
#endif

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.File(nameof(IFileSystem.IFile.Move))]
	public void Move_ReadOnly_ShouldMoveFile(
		string sourceName, string destinationName, string contents)
	{
		FileSystem.File.WriteAllText(sourceName, contents);
		FileSystem.File.SetAttributes(sourceName, FileAttributes.ReadOnly);

		FileSystem.File.Move(sourceName, destinationName);

		FileSystem.File.Exists(sourceName).Should().BeFalse();
		FileSystem.File.Exists(destinationName).Should().BeTrue();
		FileSystem.File.GetAttributes(destinationName)
		   .Should().HaveFlag(FileAttributes.ReadOnly);
		FileSystem.File.ReadAllText(destinationName).Should().Be(contents);
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.File(nameof(IFileSystem.IFile.Move))]
	public void Move_ShouldNotAdjustTimes(string source, string destination)
	{
		Test.SkipIfLongRunningTestsShouldBeSkipped(FileSystem);

		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.File.WriteAllText(source, "foo");
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		TimeSystem.Thread.Sleep(FileTestHelper.AdjustTimesDelay);

		FileSystem.File.Move(source, destination);

		DateTime creationTime = FileSystem.File.GetCreationTimeUtc(destination);
		DateTime lastAccessTime = FileSystem.File.GetLastAccessTimeUtc(destination);
		DateTime lastWriteTime = FileSystem.File.GetLastWriteTimeUtc(destination);

		creationTime.Should()
		   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
		   .BeOnOrBefore(creationTimeEnd);
		lastAccessTime.Should()
		   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
		   .BeOnOrBefore(creationTimeEnd);
		lastWriteTime.Should()
		   .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance()).And
		   .BeOnOrBefore(creationTimeEnd);
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.File(nameof(IFileSystem.IFile.Move))]
	public void Move_ShouldMoveFileWithContent(
		string sourceName, string destinationName, string contents)
	{
		FileSystem.File.WriteAllText(sourceName, contents);

		FileSystem.File.Move(sourceName, destinationName);

		FileSystem.File.Exists(sourceName).Should().BeFalse();
		FileSystem.File.Exists(destinationName).Should().BeTrue();
		FileSystem.File.ReadAllText(destinationName).Should().Be(contents);
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.File(nameof(IFileSystem.IFile.Move))]
	public void Move_SourceLocked_ShouldThrowIOException(
		string sourceName,
		string destinationName)
	{
		FileSystem.File.WriteAllText(sourceName, null);
		using FileSystemStream stream = FileSystem.File.Open(sourceName, FileMode.Open,
			FileAccess.Read, FileShare.Read);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.Move(sourceName, destinationName);
		});

		if (Test.RunsOnWindows)
		{
			exception.Should().BeOfType<IOException>();
			FileSystem.File.Exists(destinationName).Should().BeFalse();
		}
		else
		{
			FileSystem.File.Exists(sourceName).Should().BeFalse();
			FileSystem.File.Exists(destinationName).Should().BeTrue();
		}
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.File(nameof(IFileSystem.IFile.Move))]
	public void Move_SourceMissing_ShouldThrowFileNotFoundException(
		string sourceName,
		string destinationName)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.Move(sourceName, destinationName);
		});

		exception.Should().BeOfType<FileNotFoundException>()
		   .Which.Message.Should()
		   .Contain($"'{FileSystem.Path.GetFullPath(sourceName)}'");
		FileSystem.File.Exists(destinationName).Should().BeFalse();
	}
}