using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.File;

public abstract partial class FileSystemFileTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void Open_ExistingFileWithCreateNewMode_ShouldThrowFileNotFoundException(
		string path)
	{
		FileSystem.File.WriteAllText(path, null);

		Exception? exception = Record.Exception(() =>
		{
			_ = FileSystem.File.Open(path, FileMode.CreateNew);
		});

		exception.Should().BeOfType<IOException>()
		   .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
	}

	[SkippableTheory]
	[AutoData]
	public void Open_MissingFileAndIncorrectMode_ShouldThrowFileNotFoundException(
		string path)
	{
		Exception? exception = Record.Exception(() =>
		{
			_ = FileSystem.File.Open(path, FileMode.Open);
		});

		exception.Should().BeOfType<FileNotFoundException>()
		   .Which.Message.Should().Contain($"'{FileSystem.Path.GetFullPath(path)}'");
	}

	[SkippableTheory]
	[InlineAutoData(FileMode.Open, FileAccess.Read)]
	[InlineAutoData(FileMode.OpenOrCreate, FileAccess.ReadWrite)]
	public void Open_ShouldNotAdjustTimes(FileMode mode, FileAccess access, string path)
	{
		Test.SkipIfLongRunningTestsShouldBeSkipped(FileSystem);

		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.File.WriteAllText(path, "foo");
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		TimeSystem.Thread.Sleep(FileTestHelper.AdjustTimesDelay);

		FileSystemStream stream = FileSystem.File.Open(path, mode, access);
		stream.Dispose();

		DateTime creationTime = FileSystem.File.GetCreationTimeUtc(path);
		DateTime lastAccessTime = FileSystem.File.GetLastAccessTimeUtc(path);
		DateTime lastWriteTime = FileSystem.File.GetLastWriteTimeUtc(path);

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
	[InlineAutoData(FileMode.Append, FileAccess.Write)]
	[InlineAutoData(FileMode.Open, FileAccess.ReadWrite)]
	[InlineAutoData(FileMode.Create, FileAccess.ReadWrite)]
	public void Open_ShouldUseExpectedAccessDependingOnMode(
		FileMode mode,
		FileAccess expectedAccess,
		string path)
	{
		FileSystem.File.WriteAllText(path, null);

		using FileSystemStream stream = FileSystem.File.Open(path, mode);

		FileTestHelper.CheckFileAccess(stream).Should().Be(expectedAccess);
		FileTestHelper.CheckFileShare(FileSystem, path).Should().Be(FileShare.None);
	}

	[SkippableTheory]
	[InlineAutoData(FileAccess.Read, FileShare.Write)]
	[InlineAutoData(FileAccess.Write, FileShare.Read)]
	public void Open_ShouldUseGivenAccessAndShare(string path,
	                                              FileAccess access,
	                                              FileShare share)
	{
		FileSystem.File.WriteAllText(path, null);

		using FileSystemStream stream =
			FileSystem.File.Open(path, FileMode.Open, access, share);

		FileTestHelper.CheckFileAccess(stream).Should().Be(access);
		FileTestHelper.CheckFileShare(FileSystem, path).Should().Be(share);
	}

	[SkippableTheory]
	[AutoData]
	public void Open_ShouldUseNoneShareAsDefault(string path,
	                                             FileAccess access)
	{
		FileSystem.File.WriteAllText(path, null);

		using FileSystemStream stream = FileSystem.File.Open(path, FileMode.Open, access);

		FileTestHelper.CheckFileAccess(stream).Should().Be(access);
		FileTestHelper.CheckFileShare(FileSystem, path).Should().Be(FileShare.None);
	}

#if FEATURE_FILESYSTEM_STREAM_OPTIONS
	[SkippableTheory]
	[InlineAutoData(FileAccess.Read, FileShare.Write)]
	[InlineAutoData(FileAccess.Write, FileShare.Read)]
	
	public void Open_WithFileStreamOptions_ShouldUseGivenAccessAndShare(
		string path,
		FileAccess access,
		FileShare share)
	{
		FileSystem.File.WriteAllText(path, null);
		FileStreamOptions options = new()
		{
			Mode = FileMode.Open, Access = access, Share = share
		};

		using FileSystemStream stream = FileSystem.File.Open(path, options);

		FileTestHelper.CheckFileAccess(stream).Should().Be(access);
		FileTestHelper.CheckFileShare(FileSystem, path).Should().Be(share);
	}
#endif
}