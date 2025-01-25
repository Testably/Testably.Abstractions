using AutoFixture;
using System.IO;
using System.Text;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class ReadAllLinesTests
{
	[Theory]
	[AutoData]
	public void ReadAllLines_Empty_ShouldReturnEmptyArray(string path)
	{
		FileSystem.File.WriteAllText(path, "");

		string[] results = FileSystem.File.ReadAllLines(path);

		results.Should().BeEmpty();
	}

	[Theory]
	[AutoData]
	public void ReadAllLines_MissingFile_ShouldThrowFileNotFoundException(string path)
	{
		Exception? exception = Record.Exception(() =>
		{
			_ = FileSystem.File.ReadAllLines(path);
		});

		exception.Should().BeException<FileNotFoundException>(
			$"'{FileSystem.Path.GetFullPath(path)}'",
			hResult: -2147024894);
	}

	[Theory]
	[AutoData]
	public void ReadAllLines_ShouldEnumerateLines(string path, string[] lines)
	{
		string contents = string.Join(Environment.NewLine, lines);
		FileSystem.File.WriteAllText(path, contents);

		string[] results = FileSystem.File.ReadAllLines(path);

		results.Should().BeEquivalentTo(lines, o => o.WithStrictOrdering());
	}

	[Theory]
	[AutoData]
	public void ReadAllLines_ShouldNotReturnByteOrderMark(string path, string content)
	{
		FileSystem.File.WriteAllLines(path, [content], Encoding.UTF32);

		string[] result = FileSystem.File.ReadAllLines(path, Encoding.UTF32);

		//Ensure that the file contains the BOM-Bytes
		FileSystem.File.ReadAllBytes(path).Length
			.Should().BeGreaterThan(content.Length);
		result.Length.Should().Be(1);
		result[0].Should().Be(content);
	}

	[Theory]
	[ClassData(typeof(TestDataGetEncodingDifference))]
	public void ReadAllLines_WithDifferentEncoding_ShouldNotReturnWrittenText(
		string specialLine, Encoding writeEncoding, Encoding readEncoding)
	{
		string path = new Fixture().Create<string>();
		string[] lines = new Fixture().Create<string[]>();
		lines[1] = specialLine;
		string contents = string.Join(Environment.NewLine, lines);
		FileSystem.File.WriteAllText(path, contents, writeEncoding);

		string[] result = FileSystem.File.ReadAllLines(path, readEncoding);

		result.Should().NotBeEquivalentTo(lines,
			$"{contents} should be different when encoding from {writeEncoding} to {readEncoding}.");
		result[0].Should().Be(lines[0]);
	}
}
