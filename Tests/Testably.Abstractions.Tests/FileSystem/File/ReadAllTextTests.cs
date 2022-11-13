using AutoFixture;
using System.IO;
using System.Text;

namespace Testably.Abstractions.Tests.FileSystem.File;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class ReadAllTextTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void ReadAllText_FilenameNotOnWindows_ShouldBeCaseSensitive(
		string path, string contents1, string contents2)
	{
		Skip.If(Test.RunsOnWindows,
			"File names are case-insensitive only on Windows.");

		FileSystem.File.WriteAllText(path.ToUpperInvariant(), contents1);
		FileSystem.File.WriteAllText(path.ToLowerInvariant(), contents2);

		string result = FileSystem.File.ReadAllText(path.ToLowerInvariant());

		result.Should().Be(contents2);
	}

	[SkippableTheory]
	[AutoData]
	public void ReadAllText_FilenameOnWindows_ShouldBeCaseInsensitive(
		string path, string contents)
	{
		Skip.IfNot(Test.RunsOnWindows,
			"File names are case-insensitive only on Windows.");

		FileSystem.File.WriteAllText(path.ToUpperInvariant(), contents);

		string result = FileSystem.File.ReadAllText(path.ToLowerInvariant());

		result.Should().Be(contents);
	}

	[SkippableTheory]
	[AutoData]
	public void ReadAllText_MissingFile_ShouldThrowFileNotFoundException(string path)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.ReadAllText(path);
		});

		exception.Should()
		         .BeOfType<FileNotFoundException>()
		         .Which.HResult.Should()
		         .Be(-2147024894);
		exception.Should()
		         .BeOfType<FileNotFoundException>()
		         .Which.Message.Should()
		         .Contain($"'{FileSystem.Path.GetFullPath(path)}'");
	}

	[SkippableTheory]
	[AutoData]
	public void ReadAllText_ShouldAdjustTimes(string path, string contents)
	{
		Skip.If(Test.IsNetFramework && FileSystem is RealFileSystem,
			"Works unreliable on .NET Framework");
		Test.SkipIfLongRunningTestsShouldBeSkipped(FileSystem);

		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.File.WriteAllText(path, contents);
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		TimeSystem.Thread.Sleep(FileTestHelper.AdjustTimesDelay);
		DateTime updateTime = TimeSystem.DateTime.UtcNow;

		_ = FileSystem.File.ReadAllText(path);

		DateTime creationTime = FileSystem.File.GetCreationTimeUtc(path);
		DateTime lastAccessTime = FileSystem.File.GetLastAccessTimeUtc(path);
		DateTime lastWriteTime = FileSystem.File.GetLastWriteTimeUtc(path);

		if (Test.RunsOnWindows)
		{
			creationTime.Should()
			            .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance())
			            .And
			            .BeOnOrBefore(creationTimeEnd);
		}

		lastAccessTime.Should()
		              .BeOnOrAfter(updateTime.ApplySystemClockTolerance());
		lastWriteTime.Should()
		             .BeOnOrAfter(creationTimeStart.ApplySystemClockTolerance())
		             .And
		             .BeOnOrBefore(creationTimeEnd);
	}

	[SkippableTheory]
	[AutoData]
	public void ReadAllText_ShouldTolerateAltDirectorySeparatorChar(
		string contents, string directory, string fileName)
	{
		FileSystem.Directory.CreateDirectory(directory);
		string filePath = $"{directory}{FileSystem.Path.DirectorySeparatorChar}{fileName}";
		string altFilePath = $"{directory}{FileSystem.Path.AltDirectorySeparatorChar}{fileName}";
		FileSystem.File.WriteAllText(filePath, contents);

		string result = FileSystem.File.ReadAllText(altFilePath);

		result.Should().Be(contents);
	}

	[SkippableTheory]
	[ClassData(typeof(TestDataGetEncodingDifference))]
	public void ReadAllText_WithDifferentEncoding_ShouldNotReturnWrittenText(
		string contents, Encoding writeEncoding, Encoding readEncoding)
	{
		string path = new Fixture().Create<string>();
		FileSystem.File.WriteAllText(path, contents, writeEncoding);

		string result = FileSystem.File.ReadAllText(path, readEncoding);

		result.Should()
		      .NotBe(contents,
			       $"{contents} should be different when encoding from {writeEncoding} to {readEncoding}.");
	}

	[SkippableTheory]
	[AutoData]
	public void ReadAllText_WithStarCharacter_ShouldThrowFileNotFoundException(
		string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.ReadAllText(path.Substring(0, 3) + "*" + path.Substring(8));
		});

		exception.Should().NotBeNull();
	}
}