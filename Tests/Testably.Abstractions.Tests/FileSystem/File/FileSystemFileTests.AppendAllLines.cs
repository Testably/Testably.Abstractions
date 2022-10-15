using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Testably.Abstractions.Tests.FileSystem.File;

public abstract partial class FileSystemFileTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void AppendAllLines_ExistingFile_ShouldAppendLinesToFile(
		string path, List<string> previousContents, List<string> contents)
	{
		FileSystem.File.AppendAllLines(path, previousContents);

		FileSystem.File.AppendAllLines(path, contents);

		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.ReadAllLines(path).Should()
		   .BeEquivalentTo(previousContents.Concat(contents));
	}

	[SkippableTheory]
	[AutoData]
	public void AppendAllLines_MissingFile_ShouldCreateFile(
		string path, List<string> contents)
	{
		FileSystem.File.AppendAllLines(path, contents);

		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.ReadAllLines(path).Should().BeEquivalentTo(contents);
	}

	[SkippableTheory]
	[AutoData]
	public void AppendAllLines_ShouldEndWithNewline(string path)
	{
		string[] contents = { "foo", "bar" };
		string expectedResult = "foo" + Environment.NewLine + "bar" + Environment.NewLine;

		FileSystem.File.AppendAllLines(path, contents);

		FileSystem.File.ReadAllText(path).Should().BeEquivalentTo(expectedResult);
	}

	[SkippableTheory]
	[MemberAutoData(nameof(GetEncodingDifference))]
	public void AppendAllLines_WithDifferentEncoding_ShouldNotReturnWrittenText(
		string specialLine, Encoding writeEncoding, Encoding readEncoding,
		string path, string[] contents)
	{
		contents[1] = specialLine;
		FileSystem.File.AppendAllLines(path, contents, writeEncoding);

		string[] result = FileSystem.File.ReadAllLines(path, readEncoding);

		result.Should().NotBeEquivalentTo(contents,
			$"{contents} should be different when encoding from {writeEncoding} to {readEncoding}.");
		result[0].Should().Be(contents[0]);
	}
}