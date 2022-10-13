using System.IO;
using System.Text;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemFileTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	[FileSystemTests.File(nameof(IFileSystem.IFile.ReadAllText))]
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
	[FileSystemTests.File(nameof(IFileSystem.IFile.ReadAllText))]
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
	[FileSystemTests.File(nameof(IFileSystem.IFile.ReadAllText))]
	public void ReadAllText_MissingFile_ShouldThrowFileNotFoundException(string path)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.ReadAllText(path);
		});

		exception.Should().BeOfType<FileNotFoundException>()
		   .Which.Message.Should()
		   .Contain($"'{FileSystem.Path.GetFullPath(path)}'");
	}

	[SkippableTheory]
	[MemberAutoData(nameof(GetEncodingDifference))]
	[FileSystemTests.File(nameof(IFileSystem.IFile.ReadAllText))]
	public void ReadAllText_WithDifferentEncoding_ShouldNotReturnWrittenText(
		string contents, Encoding writeEncoding, Encoding readEncoding, string path)
	{
		FileSystem.File.WriteAllText(path, contents, writeEncoding);

		string result = FileSystem.File.ReadAllText(path, readEncoding);

		result.Should().NotBe(contents,
			$"{contents} should be different when encoding from {writeEncoding} to {readEncoding}.");
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.File(nameof(IFileSystem.IFile.ReadAllText))]
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