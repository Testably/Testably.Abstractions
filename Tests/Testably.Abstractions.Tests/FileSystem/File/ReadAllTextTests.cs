using AutoFixture;
using System.IO;
using System.Text;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class ReadAllTextTests
{
	[Theory]
	[AutoData]
	public async Task ReadAllText_FilenameNotOnWindows_ShouldBeCaseSensitive(
		string path, string contents1, string contents2)
	{
		Skip.If(Test.RunsOnWindows,
			"File names are case-insensitive only on Windows.");

		FileSystem.File.WriteAllText(path.ToUpperInvariant(), contents1);
		FileSystem.File.WriteAllText(path.ToLowerInvariant(), contents2);

		string result = FileSystem.File.ReadAllText(path.ToLowerInvariant());

		await That(result).IsEqualTo(contents2);
	}

	[Theory]
	[AutoData]
	public async Task ReadAllText_FilenameOnWindows_ShouldBeCaseInsensitive(
		string path, string contents)
	{
		Skip.IfNot(Test.RunsOnWindows,
			"File names are case-insensitive only on Windows.");

		FileSystem.File.WriteAllText(path.ToUpperInvariant(), contents);

		string result = FileSystem.File.ReadAllText(path.ToLowerInvariant());

		await That(result).IsEqualTo(contents);
	}

	[Theory]
	[AutoData]
	public async Task ReadAllText_MissingFile_ShouldThrowFileNotFoundException(string path)
	{
		void Act()
		{
			FileSystem.File.ReadAllText(path);
		}

		await That(Act).ThrowsExactly<FileNotFoundException>()
			.WithHResult(-2147024894).And
			.WithMessage($"*'{FileSystem.Path.GetFullPath(path)}'*").AsWildcard();
	}

	[Theory]
	[AutoData]
	public async Task ReadAllText_ShouldAdjustTimes(string path, string contents)
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
			await That(creationTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
			await That(lastAccessTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
		}
		else
		{
			await That(lastAccessTime).IsOnOrAfter(updateTime.ApplySystemClockTolerance());
		}

		await That(lastWriteTime).IsBetween(creationTimeStart).And(creationTimeEnd).Within(TimeComparison.Tolerance);
	}

	[Theory]
	[AutoData]
	public async Task ReadAllText_ShouldTolerateAltDirectorySeparatorChar(
		string contents, string directory, string fileName)
	{
		FileSystem.Directory.CreateDirectory(directory);
		string filePath = $"{directory}{FileSystem.Path.DirectorySeparatorChar}{fileName}";
		string altFilePath = $"{directory}{FileSystem.Path.AltDirectorySeparatorChar}{fileName}";
		FileSystem.File.WriteAllText(filePath, contents);

		string result = FileSystem.File.ReadAllText(altFilePath);

		await That(result).IsEqualTo(contents);
	}

	[Theory]
	[ClassData(typeof(TestDataGetEncodingDifference))]
	public async Task ReadAllText_WithDifferentEncoding_ShouldNotReturnWrittenText(
		string contents, Encoding writeEncoding, Encoding readEncoding)
	{
		string path = new Fixture().Create<string>();
		FileSystem.File.WriteAllText(path, contents, writeEncoding);

		string result = FileSystem.File.ReadAllText(path, readEncoding);

		await That(result).IsNotEqualTo(contents).Because($"{contents} should be different when encoding from {writeEncoding} to {readEncoding}.");
	}

	[Theory]
	[MemberData(nameof(GetEncodingsForReadAllText))]
	public async Task ReadAllText_WithoutReadEncoding_ShouldReturnWrittenText(
		Encoding writeEncoding)
	{
		string contents = Guid.NewGuid().ToString();
		string path = new Fixture().Create<string>();
		FileSystem.File.WriteAllText(path, contents, writeEncoding);

		string result = FileSystem.File.ReadAllText(path);

		await That(result).IsEqualTo(contents).Because($"{contents} should not be different when no read encoding is used for write encoding: {writeEncoding}.");
	}

	[Theory]
	[AutoData]
	public async Task ReadAllText_WithStarCharacter_ShouldThrowException(
		string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);

		void Act()
		{
			FileSystem.File.ReadAllText(path.Substring(0, 3) + "*" + path.Substring(8));
		}

		await That(Act).ThrowsException();
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
