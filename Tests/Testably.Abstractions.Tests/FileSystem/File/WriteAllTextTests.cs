using System.IO;
using System.Text;

namespace Testably.Abstractions.Tests.FileSystem.File;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class WriteAllTextTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void WriteAllText_MissingDirectory_ShouldThrowDirectoryNotFoundException(
		string directory, string path)
	{
		string fullPath = FileSystem.Path.Combine(directory, path);
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.WriteAllText(fullPath, "foo");
		});

		exception.Should().BeException<DirectoryNotFoundException>(
			hResult: -2147024893,
			messageContains: $"'{FileSystem.Path.GetFullPath(fullPath)}'");
	}

	[SkippableTheory]
	[AutoData]
	public void WriteAllText_PreviousFile_ShouldOverwriteFileWithText(
		string path, string contents)
	{
		FileSystem.File.WriteAllText(path, "foo");

		FileSystem.File.WriteAllText(path, contents);

		string result = FileSystem.File.ReadAllText(path);
		result.Should().Be(contents);
	}

	[SkippableTheory]
	[AutoData]
	public void WriteAllText_ShouldAdjustTimes(string path, string contents)
	{
		SkipIfLongRunningTestsShouldBeSkipped();

		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.File.WriteAllText(path, "foo");
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		TimeSystem.Thread.Sleep(FileTestHelper.AdjustTimesDelay);
		DateTime updateTime = TimeSystem.DateTime.UtcNow;

		FileSystem.File.WriteAllText(path, contents);

		DateTime creationTime = FileSystem.File.GetCreationTimeUtc(path);
		DateTime lastAccessTime = FileSystem.File.GetLastAccessTimeUtc(path);
		DateTime lastWriteTime = FileSystem.File.GetLastWriteTimeUtc(path);

		if (Test.RunsOnWindows)
		{
			creationTime.Should()
				.BeBetween(creationTimeStart, creationTimeEnd);
			lastAccessTime.Should()
				.BeOnOrAfter(updateTime.ApplySystemClockTolerance());
		}
		else
		{
			lastAccessTime.Should()
				.BeBetween(creationTimeStart, creationTimeEnd);
		}

		lastWriteTime.Should()
			.BeOnOrAfter(updateTime.ApplySystemClockTolerance());
	}

	[SkippableTheory]
	[AutoData]
	public void WriteAllText_ShouldCreateFileWithByteOrderMark(
		string path)
	{
		byte[] expectedBytes =
		[
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
		];

		FileSystem.File.WriteAllText(path, "AA", Encoding.UTF32);

		FileSystem.Should().HaveFile(path)
			.Which.HasContent(expectedBytes);
	}

	[SkippableTheory]
	[AutoData]
	public void WriteAllText_ShouldCreateFileWithText(string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		string result = FileSystem.File.ReadAllText(path);
		result.Should().Be(contents);
	}

	[SkippableTheory]
	[AutoData]
	public void WriteAllText_SpecialCharacters_ShouldReturnSameText(string path)
	{
		char[] specialCharacters =
		[
			'Ä',
			'Ö',
			'Ü',
			'ä',
			'ö',
			'ü',
			'ß'
		];
		foreach (char specialCharacter in specialCharacters)
		{
			string contents = "_" + specialCharacter;
			FileSystem.File.WriteAllText(path, contents);

			string result = FileSystem.File.ReadAllText(path);

			result.Should().Be(contents,
				$"{contents} should be encoded and decoded identical.");
		}
	}

	[SkippableTheory]
	[AutoData]
	public void WriteAllText_WhenContentIsNull_ShouldNotThrowException(string path)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.WriteAllText(path, null);
		});

		exception.Should().BeNull();
	}

	[SkippableTheory]
	[AutoData]
	public void WriteAllText_WhenDirectoryWithSameNameExists_ShouldThrowUnauthorizedAccessException(
		string path)
	{
		FileSystem.Directory.CreateDirectory(path);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.WriteAllText(path, null);
		});

		exception.Should().BeException<UnauthorizedAccessException>(
			hResult: -2147024891);
		FileSystem.Should().HaveDirectory(path);
		FileSystem.Should().NotHaveFile(path);
	}

	[SkippableTheory]
	[AutoData]
	public void WriteAllText_WhenFileIsHidden_ShouldThrowUnauthorizedAccessException_OnWindows(
		string path, string contents)
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystem.File.WriteAllText(path, null);
		FileSystem.File.SetAttributes(path, FileAttributes.Hidden);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.WriteAllText(path, contents);
		});

		exception.Should().BeException<UnauthorizedAccessException>(hResult: -2147024891);
	}
}
