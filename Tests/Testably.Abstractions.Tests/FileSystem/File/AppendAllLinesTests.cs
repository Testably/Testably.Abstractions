using AutoFixture;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Testably.Abstractions.Tests.FileSystem.File;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class AppendAllLinesTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
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
			.BeEquivalentTo(previousContents.Concat(contents),
				o => o.WithStrictOrdering());
	}

	[SkippableTheory]
	[AutoData]
	public void AppendAllLines_MissingFile_ShouldCreateFile(
		string path, List<string> contents)
	{
		FileSystem.File.AppendAllLines(path, contents);

		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.ReadAllLines(path).Should()
			.BeEquivalentTo(contents, o => o.WithStrictOrdering());
	}

	[SkippableTheory]
	[AutoData]
	public void AppendAllLines_NullContent_ShouldThrowArgumentNullException(
		string path)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.AppendAllLines(path, null!);
		});

		exception.Should().BeException<ArgumentNullException>(
			hResult: -2147467261,
			paramName: "contents");
	}

	[SkippableTheory]
	[AutoData]
	public void AppendAllLines_NullEncoding_ShouldThrowArgumentNullException(
		string path)
	{
		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.AppendAllLines(path, new List<string>(), null!);
		});

		exception.Should().BeException<ArgumentNullException>(
			hResult: -2147467261,
			paramName: "encoding");
	}

	[SkippableTheory]
	[AutoData]
	public void AppendAllLines_ShouldEndWithNewline(string path)
	{
		string[] contents =
		{
			"foo", "bar"
		};
		string expectedResult = "foo" + Environment.NewLine + "bar" + Environment.NewLine;

		FileSystem.File.AppendAllLines(path, contents);

		FileSystem.File.ReadAllText(path).Should().BeEquivalentTo(expectedResult);
	}

	[SkippableTheory]
	[ClassData(typeof(TestDataGetEncodingDifference))]
	public void AppendAllLines_WithDifferentEncoding_ShouldNotReturnWrittenText(
		string specialLine, Encoding writeEncoding, Encoding readEncoding)
	{
		string path = new Fixture().Create<string>();
		string[] lines = new Fixture().Create<string[]>();
		lines[1] = specialLine;
		FileSystem.File.AppendAllLines(path, lines, writeEncoding);

		string[] result = FileSystem.File.ReadAllLines(path, readEncoding);

		result.Should().NotBeEquivalentTo(lines,
			$"{lines} should be different when encoding from {writeEncoding} to {readEncoding}.");
		result[0].Should().Be(lines[0]);
	}
}
