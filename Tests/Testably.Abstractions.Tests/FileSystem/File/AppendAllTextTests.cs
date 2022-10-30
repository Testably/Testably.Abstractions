using System.Text;

namespace Testably.Abstractions.Tests.FileSystem.File;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class AppendAllTextTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void AppendAllText_ExistingFile_ShouldAppendLinesToFile(
		string path, string previousContents, string contents)
	{
		FileSystem.File.AppendAllText(path, previousContents);

		FileSystem.File.AppendAllText(path, contents);

		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.ReadAllText(path).Should()
		   .BeEquivalentTo(previousContents + contents);
	}

	[SkippableTheory]
	[AutoData]
	public void AppendAllText_MissingFile_ShouldCreateFile(
		string path, string contents)
	{
		FileSystem.File.AppendAllText(path, contents);

		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.ReadAllLines(path).Should().BeEquivalentTo(contents);
	}

	[SkippableTheory]
	[AutoData]
	public void AppendAllText_ShouldAdjustTimes(string path, string contents)
	{
		Test.SkipIfLongRunningTestsShouldBeSkipped(FileSystem);

		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.File.WriteAllText(path, "foo");
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		TimeSystem.Thread.Sleep(FileTestHelper.AdjustTimesDelay);
		DateTime updateTime = TimeSystem.DateTime.UtcNow;

		FileSystem.File.AppendAllText(path, contents);

		DateTime creationTime = FileSystem.File.GetCreationTimeUtc(path);
		DateTime lastAccessTime = FileSystem.File.GetLastAccessTimeUtc(path);
		DateTime lastWriteTime = FileSystem.File.GetLastWriteTimeUtc(path);

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
	public void AppendAllText_ShouldNotEndWithNewline(string path)
	{
		string contents = "foo";

		FileSystem.File.AppendAllText(path, contents);

		FileSystem.File.ReadAllText(path).Should().BeEquivalentTo(contents);
	}

	[SkippableTheory]
	[ClassData(typeof(TestDataGetEncodingDifference))]
	public void AppendAllText_WithDifferentEncoding_ShouldNotReturnWrittenText(
		string contents, Encoding writeEncoding, Encoding readEncoding,
		string path)
	{
		FileSystem.File.AppendAllText(path, contents, writeEncoding);

		string[] result = FileSystem.File.ReadAllLines(path, readEncoding);

		result.Should().NotBeEquivalentTo(contents,
			$"{contents} should be different when encoding from {writeEncoding} to {readEncoding}.");
	}
}