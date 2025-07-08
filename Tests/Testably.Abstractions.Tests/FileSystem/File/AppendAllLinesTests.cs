using AutoFixture;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class AppendAllLinesTests
{
	[Theory]
	[AutoData]
	public void AppendAllLines_ExistingFile_ShouldAppendLinesToFile(
		string path, List<string> previousContents, List<string> contents)
	{
		string expectedContent = string.Join(Environment.NewLine, previousContents.Concat(contents))
		                         + Environment.NewLine;
		FileSystem.File.AppendAllLines(path, previousContents);

		FileSystem.File.AppendAllLines(path, contents);

		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.ReadAllText(path).Should().BeEquivalentTo(expectedContent);
	}

	[Theory]
	[AutoData]
	public void AppendAllLines_MissingFile_ShouldCreateFile(
		string path, List<string> contents)
	{
		string expectedContent = string.Join(Environment.NewLine, contents)
		                         + Environment.NewLine;
		FileSystem.File.AppendAllLines(path, contents);

		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.ReadAllText(path).Should().BeEquivalentTo(expectedContent);
	}

	[Theory]
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

	[Theory]
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

	[Theory]
	[AutoData]
	public void AppendAllLines_ShouldEndWithNewline(string path)
	{
		string[] contents = ["foo", "bar"];
		string expectedResult = "foo" + Environment.NewLine + "bar" + Environment.NewLine;

		FileSystem.File.AppendAllLines(path, contents);

		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.ReadAllText(path).Should().BeEquivalentTo(expectedResult);
	}

	[Theory]
	[AutoData]
	public void
		AppendAllLines_WhenDirectoryWithSameNameExists_ShouldThrowUnauthorizedAccessException(
			string path, string[] contents)
	{
		FileSystem.Directory.CreateDirectory(path);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.AppendAllLines(path, contents);
		});

		exception.Should().BeException<UnauthorizedAccessException>(
			hResult: -2147024891);
		FileSystem.Directory.Exists(path).Should().BeTrue();
		FileSystem.File.Exists(path).Should().BeFalse();
	}

	[Theory]
	[ClassData(typeof(TestDataGetEncodingDifference))]
	public async Task AppendAllLines_WithDifferentEncoding_ShouldNotReturnWrittenText(
		string specialLine, Encoding writeEncoding, Encoding readEncoding)
	{
		string path = new Fixture().Create<string>();
		string[] lines = new Fixture().Create<string[]>();
		lines[1] = specialLine;
		FileSystem.File.AppendAllLines(path, lines, writeEncoding);

		string[] result = FileSystem.File.ReadAllLines(path, readEncoding);

		await That(result).IsNotEqualTo(lines).InAnyOrder();
		await That(result[0]).IsEqualTo(lines[0]);
	}
}
