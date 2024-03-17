using AutoFixture;
using System.IO;
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

		FileSystem.Should().HaveFile(path)
			.Which.HasContent(previousContents + contents);
	}

	[SkippableTheory]
	[AutoData]
	public void AppendAllText_MissingDirectory_ShouldThrowDirectoryNotFoundException(
		string missingPath, string fileName, string contents)
	{
		string filePath = FileSystem.Path.Combine(missingPath, fileName);
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.AppendAllText(filePath, contents);
		});

		exception.Should().BeException<DirectoryNotFoundException>(hResult: -2147024893);
	}

	[SkippableTheory]
	[AutoData]
	public void AppendAllText_MissingFile_ShouldCreateFile(
		string path, string contents)
	{
		FileSystem.File.AppendAllText(path, contents);

		FileSystem.Should().HaveFile(path)
			.Which.HasContent(contents);
	}

	[SkippableTheory]
	[AutoData]
	public void AppendAllText_MissingFile_ShouldCreateFileWithByteOrderMark(
		string path)
	{
		byte[] expectedBytes =
		{
			255,
			254,
			0,
			0,
			65,
			0,
			0,
			0,
			65,
			0,
			0,
			0
		};

		FileSystem.File.AppendAllText(path, "AA", Encoding.UTF32);

		FileSystem.Should().HaveFile(path)
			.Which.HasContent(expectedBytes);
	}

	[SkippableTheory]
	[AutoData]
	public void AppendAllText_ShouldAdjustTimes(string path, string contents)
	{
		Skip.If(LongRunningTestsShouldBeSkipped());

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

		FileSystem.Should().HaveFile(path)
			.Which.HasContent(contents);
	}

	[SkippableTheory]
	[AutoData]
	public void
		AppendAllText_WhenDirectoryWithSameNameExists_ShouldThrowUnauthorizedAccessException(
			string path)
	{
		FileSystem.Directory.CreateDirectory(path);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.AppendAllText(path, null);
		});

		exception.Should().BeException<UnauthorizedAccessException>(
			hResult: -2147024891);
		FileSystem.Should().HaveDirectory(path);
		FileSystem.Should().NotHaveFile(path);
	}

	[SkippableTheory]
	[AutoData]
	public void AppendAllText_WhenFileIsHidden_ShouldNotThrowException(
		string path, string contents)
	{
		FileSystem.File.WriteAllText(path, "some content");
		FileSystem.File.SetAttributes(path, FileAttributes.Hidden);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.AppendAllText(path, contents);
		});

		exception.Should().BeNull();
	}

	[SkippableTheory]
	[ClassData(typeof(TestDataGetEncodingDifference))]
	public void AppendAllText_WithDifferentEncoding_ShouldNotReturnWrittenText(
		string contents, Encoding writeEncoding, Encoding readEncoding)
	{
		string path = new Fixture().Create<string>();
		FileSystem.File.AppendAllText(path, contents, writeEncoding);

		string[] result = FileSystem.File.ReadAllLines(path, readEncoding);

		result.Should().NotBeEquivalentTo(contents,
			$"{contents} should be different when encoding from {writeEncoding} to {readEncoding}.");
	}
}
