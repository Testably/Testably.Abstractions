using AutoFixture;
using System.IO;
using System.Text;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class ReadAllTextTests
{
	[Theory]
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

	[Theory]
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

	[Theory]
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

	[Theory]
	[AutoData]
	public void ReadAllText_ShouldAdjustTimes(string path, string contents)
	{
		Skip.If(Test.IsNetFramework && FileSystem is RealFileSystem,
			"Works unreliable on .NET Framework");
		SkipIfLongRunningTestsShouldBeSkipped();

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
				.BeBetween(creationTimeStart, creationTimeEnd);
			lastAccessTime.Should()
				.BeBetween(creationTimeStart, creationTimeEnd);
		}
		else
		{
			lastAccessTime.Should()
				.BeOnOrAfter(updateTime.ApplySystemClockTolerance());
		}

		lastWriteTime.Should()
			.BeBetween(creationTimeStart, creationTimeEnd);
	}

	[Theory]
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

	[Theory]
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

	[Theory]
	[MemberData(nameof(GetEncodingsForReadAllText))]
	public void ReadAllText_WithoutReadEncoding_ShouldReturnWrittenText(
		Encoding writeEncoding)
	{
		string contents = Guid.NewGuid().ToString();
		string path = new Fixture().Create<string>();
		FileSystem.File.WriteAllText(path, contents, writeEncoding);

		string result = FileSystem.File.ReadAllText(path);

		result.Should().Be(contents,
			$"{contents} should not be different when no read encoding is used for write encoding: {writeEncoding}.");
	}

	[Theory]
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

	#region Helpers

	#pragma warning disable MA0018
	public static TheoryData<Encoding> GetEncodingsForReadAllText()
		=> new()
		{
			(Encoding)new UTF32Encoding(false, true, true),
			// big endian
			(Encoding)new UTF32Encoding(true, true, true),
			(Encoding)new UTF8Encoding(true, true),
			(Encoding)new ASCIIEncoding(),
		};
	#pragma warning restore MA0018

	#endregion
}
