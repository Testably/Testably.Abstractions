using AutoFixture;
using System.IO;
using System.Text;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class AppendAllTextTests
{
	[Theory]
	[AutoData]
	public void AppendAllText_ExistingFile_ShouldAppendLinesToFile(
		string path, string previousContents, string contents)
	{
		FileSystem.File.AppendAllText(path, previousContents);

		FileSystem.File.AppendAllText(path, contents);

		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.ReadAllText(path).Should().BeEquivalentTo(previousContents + contents);
	}

	[Theory]
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

	[Theory]
	[AutoData]
	public void AppendAllText_MissingFile_ShouldCreateFile(
		string path, string contents)
	{
		FileSystem.File.AppendAllText(path, contents);

		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.ReadAllText(path).Should().BeEquivalentTo(contents);
	}

	[Theory]
	[AutoData]
	public void AppendAllText_MissingFile_ShouldCreateFileWithByteOrderMark(
		string path)
	{
		byte[] expectedBytes = [255, 254, 0, 0, 65, 0, 0, 0, 65, 0, 0, 0];

		FileSystem.File.AppendAllText(path, "AA", Encoding.UTF32);

		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.ReadAllBytes(path).Should().BeEquivalentTo(expectedBytes);
	}

	[Theory]
	[AutoData]
	public async Task AppendAllText_ShouldAdjustTimes(string path, string contents)
	{
		SkipIfLongRunningTestsShouldBeSkipped();

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
				.BeBetween(creationTimeStart, creationTimeEnd);
			await That(lastAccessTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());
		}
		else
		{
			lastAccessTime.Should()
				.BeBetween(creationTimeStart, creationTimeEnd);
		}

		await That(lastWriteTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());
	}

	[Theory]
	[AutoData]
	public void AppendAllText_ShouldNotEndWithNewline(string path)
	{
		string contents = "foo";

		FileSystem.File.AppendAllText(path, contents);

		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.ReadAllText(path).Should().BeEquivalentTo(contents);
	}

	[Theory]
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
		FileSystem.Directory.Exists(path).Should().BeTrue();
		FileSystem.File.Exists(path).Should().BeFalse();
	}

	[Theory]
	[AutoData]
	public async Task AppendAllText_WhenFileIsHidden_ShouldNotThrowException(
		string path, string contents)
	{
		FileSystem.File.WriteAllText(path, "some content");
		FileSystem.File.SetAttributes(path, FileAttributes.Hidden);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.AppendAllText(path, contents);
		});

		await That(exception).IsNull();
	}

	[Theory]
	[ClassData(typeof(TestDataGetEncodingDifference))]
	public async Task AppendAllText_WithDifferentEncoding_ShouldNotReturnWrittenText(
		string contents, Encoding writeEncoding, Encoding readEncoding)
	{
		string path = new Fixture().Create<string>();
		FileSystem.File.AppendAllText(path, contents, writeEncoding);

		string[] result = FileSystem.File.ReadAllLines(path, readEncoding);

		await That(result).IsNotEqualTo([contents]);
	}

#if FEATURE_FILE_SPAN
	[Theory]
	[AutoData]
	public void AppendAllText_Span_ExistingFile_ShouldAppendLinesToFile(
		string path, string previousContents, string contents)
	{
		FileSystem.File.AppendAllText(path, previousContents);

		FileSystem.File.AppendAllText(path, contents.AsSpan());

		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.ReadAllText(path).Should().BeEquivalentTo(previousContents + contents);
	}

	[Theory]
	[AutoData]
	public void AppendAllText_Span_MissingDirectory_ShouldThrowDirectoryNotFoundException(
		string missingPath, string fileName, string contents)
	{
		string filePath = FileSystem.Path.Combine(missingPath, fileName);
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.AppendAllText(filePath, contents.AsSpan());
		});

		exception.Should().BeException<DirectoryNotFoundException>(hResult: -2147024893);
	}

	[Theory]
	[AutoData]
	public void AppendAllText_Span_MissingFile_ShouldCreateFile(
		string path, string contents)
	{
		FileSystem.File.AppendAllText(path, contents.AsSpan());

		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.ReadAllText(path).Should().BeEquivalentTo(contents);
	}

	[Theory]
	[AutoData]
	public void AppendAllText_Span_MissingFile_ShouldCreateFileWithByteOrderMark(
		string path)
	{
		byte[] expectedBytes = [255, 254, 0, 0, 65, 0, 0, 0, 65, 0, 0, 0];

		FileSystem.File.AppendAllText(path, "AA".AsSpan(), Encoding.UTF32);

		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.ReadAllBytes(path).Should().BeEquivalentTo(expectedBytes);
	}

	[Theory]
	[AutoData]
	public async Task AppendAllText_Span_ShouldAdjustTimes(string path, string contents)
	{
		SkipIfLongRunningTestsShouldBeSkipped();

		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.File.WriteAllText(path, "foo");
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		TimeSystem.Thread.Sleep(FileTestHelper.AdjustTimesDelay);
		DateTime updateTime = TimeSystem.DateTime.UtcNow;

		FileSystem.File.AppendAllText(path, contents.AsSpan());

		DateTime creationTime = FileSystem.File.GetCreationTimeUtc(path);
		DateTime lastAccessTime = FileSystem.File.GetLastAccessTimeUtc(path);
		DateTime lastWriteTime = FileSystem.File.GetLastWriteTimeUtc(path);

		if (Test.RunsOnWindows)
		{
			creationTime.Should()
				.BeBetween(creationTimeStart, creationTimeEnd);
			await That(lastAccessTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());
		}
		else
		{
			lastAccessTime.Should()
				.BeBetween(creationTimeStart, creationTimeEnd);
		}

		await That(lastWriteTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());
	}

	[Theory]
	[AutoData]
	public void AppendAllText_Span_ShouldNotEndWithNewline(string path)
	{
		string contents = "foo";

		FileSystem.File.AppendAllText(path, contents.AsSpan());

		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.ReadAllText(path).Should().BeEquivalentTo(contents);
	}

	[Theory]
	[AutoData]
	public void
		AppendAllText_Span_WhenDirectoryWithSameNameExists_ShouldThrowUnauthorizedAccessException(
			string path)
	{
		FileSystem.Directory.CreateDirectory(path);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.AppendAllText(path, "".AsSpan());
		});

		exception.Should().BeException<UnauthorizedAccessException>(
			hResult: -2147024891);
		FileSystem.Directory.Exists(path).Should().BeTrue();
		FileSystem.File.Exists(path).Should().BeFalse();
	}

	[Theory]
	[AutoData]
	public async Task AppendAllText_Span_WhenFileIsHidden_ShouldNotThrowException(
		string path, string contents)
	{
		FileSystem.File.WriteAllText(path, "some content");
		FileSystem.File.SetAttributes(path, FileAttributes.Hidden);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.AppendAllText(path, contents.AsSpan());
		});

		await That(exception).IsNull();
	}

	[Theory]
	[ClassData(typeof(TestDataGetEncodingDifference))]
	public async Task AppendAllText_Span_WithDifferentEncoding_ShouldNotReturnWrittenText(
		string contents, Encoding writeEncoding, Encoding readEncoding)
	{
		string path = new Fixture().Create<string>();
		FileSystem.File.AppendAllText(path, contents.AsSpan(), writeEncoding);

		string[] result = FileSystem.File.ReadAllLines(path, readEncoding);

		await That(result).IsNotEqualTo([contents]);
	}
#endif
}
