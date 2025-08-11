using System.IO;
using System.Text;
using Testably.Abstractions.Testing.FileSystem;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class WriteAllTextTests
{
	[Theory]
	[AutoData]
	public async Task WriteAllText_MissingDirectory_ShouldThrowDirectoryNotFoundException(
		string directory, string path)
	{
		string fullPath = FileSystem.Path.Combine(directory, path);

		void Act()
		{
			FileSystem.File.WriteAllText(fullPath, "foo");
		}

		await That(Act).Throws<DirectoryNotFoundException>()
			.WithHResult(-2147024893).And
			.WithMessageContaining($"'{FileSystem.Path.GetFullPath(fullPath)}'");
	}

	[Theory]
	[AutoData]
	public async Task WriteAllText_PreviousFile_ShouldOverwriteFileWithText(
		string path, string contents)
	{
		FileSystem.File.WriteAllText(path, "foo");

		FileSystem.File.WriteAllText(path, contents);

		string result = FileSystem.File.ReadAllText(path);
		await That(result).IsEqualTo(contents);
	}

	[Theory]
	[AutoData]
	public async Task WriteAllText_ShouldAdjustTimes(string path, string contents)
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
			await That(creationTime).IsBetween(creationTimeStart).And(creationTimeEnd)
				.Within(TimeComparison.Tolerance);
			await That(lastAccessTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());
		}
		else
		{
			await That(lastAccessTime).IsBetween(creationTimeStart).And(creationTimeEnd)
				.Within(TimeComparison.Tolerance);
		}

		await That(lastWriteTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());
	}

	[Theory]
	[AutoData]
	public async Task WriteAllText_ShouldCreateFileWithByteOrderMark(
		string path)
	{
		byte[] expectedBytes = [255, 254, 0, 0, 65, 0, 0, 0, 65, 0, 0, 0];

		FileSystem.File.WriteAllText(path, "AA", Encoding.UTF32);

		await That(FileSystem.File.Exists(path)).IsTrue();
		await That(FileSystem.File.ReadAllBytes(path)).IsEqualTo(expectedBytes);
	}

	[Theory]
	[AutoData]
	public async Task WriteAllText_ShouldCreateFileWithText(string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		string result = FileSystem.File.ReadAllText(path);
		await That(result).IsEqualTo(contents);
	}

	[Theory]
	[AutoData]
	public async Task WriteAllText_SpecialCharacters_ShouldReturnSameText(string path)
	{
		char[] specialCharacters =
		[
			'Ä',
			'Ö',
			'Ü',
			'ä',
			'ö',
			'ü',
			'ß',
		];
		foreach (char specialCharacter in specialCharacters)
		{
			string contents = "_" + specialCharacter;
			FileSystem.File.WriteAllText(path, contents);

			string result = FileSystem.File.ReadAllText(path);

			await That(result).IsEqualTo(contents)
				.Because($"{contents} should be encoded and decoded identical.");
		}
	}

	[Theory]
	[AutoData]
	public async Task WriteAllText_WhenContentIsNull_ShouldNotThrowException(string path)
	{
		void Act()
		{
			FileSystem.File.WriteAllText(path, null);
		}

		await That(Act).DoesNotThrow();
	}

	[Theory]
	[AutoData]
	public async Task
		WriteAllText_WhenDirectoryWithSameNameExists_ShouldThrowUnauthorizedAccessException(
			string path)
	{
		FileSystem.Directory.CreateDirectory(path);

		void Act()
		{
			FileSystem.File.WriteAllText(path, null);
		}

		await That(Act).Throws<UnauthorizedAccessException>().WithHResult(-2147024891);
		await That(FileSystem.Directory.Exists(path)).IsTrue();
		await That(FileSystem.File.Exists(path)).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task
		WriteAllText_WhenFileIsHidden_ShouldThrowUnauthorizedAccessException_OnWindows(
			string path, string contents)
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystem.File.WriteAllText(path, null);
		FileSystem.File.SetAttributes(path, FileAttributes.Hidden);

		void Act()
		{
			FileSystem.File.WriteAllText(path, contents);
		}

		await That(Act).Throws<UnauthorizedAccessException>().WithHResult(-2147024891);
	}

	[Fact]
	public async Task
		WriteAllText_WithoutAccessRightsToParentDirectory_ShouldThrowUnauthorizedAccessException()
	{
		Skip.IfNot(Test.RunsOnWindows);

		string folderPath = @"C:\Program Files";
		if (FileSystem is MockFileSystem mockFileSystem)
		{
			mockFileSystem.Directory.CreateDirectory(folderPath);
			mockFileSystem.WithAccessControlStrategy(
				new DefaultAccessControlStrategy((p, _)
					=> !folderPath.Equals(p, StringComparison.Ordinal)));
		}

		string path = FileSystem.Path.Combine(folderPath, "my-file.txt");

		void Act()
		{
			FileSystem.File.WriteAllText(path, "some-content");
		}

		await That(Act).Throws<UnauthorizedAccessException>()
			.WithHResult(-2147024891).And
			.WithMessage($"Access to the path '{path}' is denied.");
	}

#if FEATURE_FILE_SPAN
	[Theory]
	[AutoData]
	public async Task WriteAllText_Span_MissingDirectory_ShouldThrowDirectoryNotFoundException(
		string directory, string path)
	{
		string fullPath = FileSystem.Path.Combine(directory, path);
		void Act()
		{
			FileSystem.File.WriteAllText(fullPath, "foo".AsSpan());
		}

		await That(Act).Throws<DirectoryNotFoundException>()
			.WithHResult(-2147024893).And
			.WithMessageContaining($"'{FileSystem.Path.GetFullPath(fullPath)}'");
	}

	[Theory]
	[AutoData]
	public async Task WriteAllText_Span_PreviousFile_ShouldOverwriteFileWithText(
		string path, string contents)
	{
		FileSystem.File.WriteAllText(path, "foo");

		FileSystem.File.WriteAllText(path, contents.AsSpan());

		string result = FileSystem.File.ReadAllText(path);
		await That(result).IsEqualTo(contents);
	}

	[Theory]
	[AutoData]
	public async Task WriteAllText_Span_ShouldAdjustTimes(string path, string contents)
	{
		SkipIfLongRunningTestsShouldBeSkipped();

		DateTime creationTimeStart = TimeSystem.DateTime.UtcNow;
		FileSystem.File.WriteAllText(path, "foo");
		DateTime creationTimeEnd = TimeSystem.DateTime.UtcNow;
		TimeSystem.Thread.Sleep(FileTestHelper.AdjustTimesDelay);
		DateTime updateTime = TimeSystem.DateTime.UtcNow;

		FileSystem.File.WriteAllText(path, contents.AsSpan());

		DateTime creationTime = FileSystem.File.GetCreationTimeUtc(path);
		DateTime lastAccessTime = FileSystem.File.GetLastAccessTimeUtc(path);
		DateTime lastWriteTime = FileSystem.File.GetLastWriteTimeUtc(path);

		if (Test.RunsOnWindows)
		{
			await That(creationTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
			await That(lastAccessTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());
		}
		else
		{
			await That(lastAccessTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
		}

		await That(lastWriteTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());
	}

	[Theory]
	[AutoData]
	public async Task WriteAllText_Span_ShouldCreateFileWithByteOrderMark(
		string path)
	{
		byte[] expectedBytes = [255, 254, 0, 0, 65, 0, 0, 0, 65, 0, 0, 0];

		FileSystem.File.WriteAllText(path, "AA".AsSpan(), Encoding.UTF32);

		await That(FileSystem.File.Exists(path)).IsTrue();
		await That(FileSystem.File.ReadAllBytes(path)).IsEqualTo(expectedBytes);
	}

	[Theory]
	[AutoData]
	public async Task WriteAllText_Span_ShouldCreateFileWithText(string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents.AsSpan());

		string result = FileSystem.File.ReadAllText(path);
		await That(result).IsEqualTo(contents);
	}

	[Theory]
	[AutoData]
	public async Task WriteAllText_Span_SpecialCharacters_ShouldReturnSameText(string path)
	{
		char[] specialCharacters =
		[
			'Ä',
			'Ö',
			'Ü',
			'ä',
			'ö',
			'ü',
			'ß',
		];
		foreach (char specialCharacter in specialCharacters)
		{
			string contents = "_" + specialCharacter;
			FileSystem.File.WriteAllText(path, contents.AsSpan());

			string result = FileSystem.File.ReadAllText(path);

			await That(result).IsEqualTo(contents).Because($"{contents} should be encoded and decoded identical.");
		}
	}

	[Theory]
	[AutoData]
	public async Task WriteAllText_Span_WhenDirectoryWithSameNameExists_ShouldThrowUnauthorizedAccessException(
		string path)
	{
		FileSystem.Directory.CreateDirectory(path);

		void Act()
		{
			FileSystem.File.WriteAllText(path, "".AsSpan());
		}

		await That(Act).Throws<UnauthorizedAccessException>().WithHResult(-2147024891);
		await That(FileSystem.Directory.Exists(path)).IsTrue();
		await That(FileSystem.File.Exists(path)).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task WriteAllText_Span_WhenFileIsHidden_ShouldThrowUnauthorizedAccessException_OnWindows(
		string path, string contents)
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystem.File.WriteAllText(path, null);
		FileSystem.File.SetAttributes(path, FileAttributes.Hidden);

		void Act()
		{
			FileSystem.File.WriteAllText(path, contents.AsSpan());
		}

		await That(Act).Throws<UnauthorizedAccessException>().WithHResult(-2147024891);
	}
#endif
}
