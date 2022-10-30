using System.Text;

namespace Testably.Abstractions.Tests.FileSystem.File;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class WriteAllLinesTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
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
}