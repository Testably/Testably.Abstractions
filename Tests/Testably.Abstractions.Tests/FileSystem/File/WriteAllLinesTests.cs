using System.IO;
using System.Linq;
using System.Text;

namespace Testably.Abstractions.Tests.FileSystem.File;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class WriteAllLinesTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void WriteAllLines_Enumerable_PreviousFile_ShouldOverwriteFileWithText(
		string path, string[] contents)
	{
		FileSystem.File.WriteAllText(path, "foo");

		FileSystem.File.WriteAllLines(path, contents.AsEnumerable());

		string[] result = FileSystem.File.ReadAllLines(path);
		result.Should().BeEquivalentTo(contents, o => o.WithStrictOrdering());
	}

	[SkippableTheory]
	[AutoData]
	public void WriteAllLines_Enumerable_ShouldCreateFileWithText(
		string path, string[] contents)
	{
		FileSystem.File.WriteAllLines(path, contents.AsEnumerable());

		string[] result = FileSystem.File.ReadAllLines(path);
		result.Should().BeEquivalentTo(contents, o => o.WithStrictOrdering());
	}

	[SkippableTheory]
	[AutoData]
	public void WriteAllLines_Enumerable_WithEncoding_ShouldCreateFileWithText(
		Encoding encoding, string path, string[] contents)
	{
		FileSystem.File.WriteAllLines(path, contents.AsEnumerable(), encoding);

		string[] result = FileSystem.File.ReadAllLines(path, encoding);
		result.Should().BeEquivalentTo(contents, o => o.WithStrictOrdering());
	}

	[SkippableTheory]
	[AutoData]
	public void WriteAllLines_PreviousFile_ShouldOverwriteFileWithText(
		string path, string[] contents)
	{
		FileSystem.File.WriteAllText(path, "foo");

		FileSystem.File.WriteAllLines(path, contents);

		string[] result = FileSystem.File.ReadAllLines(path);
		result.Should().BeEquivalentTo(contents, o => o.WithStrictOrdering());
	}

	[SkippableTheory]
	[AutoData]
	public void WriteAllLines_ShouldCreateFileWithText(string path, string[] contents)
	{
		FileSystem.File.WriteAllLines(path, contents);

		string[] result = FileSystem.File.ReadAllLines(path);
		result.Should().BeEquivalentTo(contents, o => o.WithStrictOrdering());
	}

	[SkippableTheory]
	[AutoData]
	public void WriteAllLines_WithEncoding_ShouldCreateFileWithText(
		Encoding encoding, string path, string[] contents)
	{
		FileSystem.File.WriteAllLines(path, contents, encoding);

		string[] result = FileSystem.File.ReadAllLines(path, encoding);
		result.Should().BeEquivalentTo(contents, o => o.WithStrictOrdering());
	}

	[SkippableTheory]
	[AutoData]
	public void WriteAllLines_WhenFileIsHidden_ShouldThrowUnauthorizedAccessException_OnWindows(
		string path, string[] contents)
	{
		Skip.IfNot(Test.RunsOnWindows);

		FileSystem.File.WriteAllText(path, null);
		FileSystem.File.SetAttributes(path, FileAttributes.Hidden);

		var exception = Record.Exception(() =>
		{
			FileSystem.File.WriteAllLines(path, contents);
		});

		exception.Should().BeException<UnauthorizedAccessException>(hResult: -2147024891);
	}
}