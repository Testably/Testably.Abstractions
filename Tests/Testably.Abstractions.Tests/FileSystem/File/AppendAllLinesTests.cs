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
	public async Task AppendAllLines_Enumerable_WithoutEncoding_ShouldUseUtf8(
		string path)
	{
		string[] contents = ["breuß"];

		FileSystem.File.AppendAllLines(path, contents.AsEnumerable());

		byte[] bytes = FileSystem.File.ReadAllBytes(path);
		await That(bytes.Length).IsEqualTo(6 + Environment.NewLine.Length);
	}

	[Theory]
	[AutoData]
	public async Task AppendAllLines_ExistingFile_ShouldAppendLinesToFile(
		string path, List<string> previousContents, List<string> contents)
	{
		string expectedContent = string.Join(Environment.NewLine, previousContents.Concat(contents))
		                         + Environment.NewLine;
		FileSystem.File.AppendAllLines(path, previousContents);

		FileSystem.File.AppendAllLines(path, contents);

		await That(FileSystem.File.Exists(path)).IsTrue();
		await That(FileSystem.File.ReadAllText(path)).IsEqualTo(expectedContent);
	}

	[Theory]
	[AutoData]
	public async Task AppendAllLines_MissingFile_ShouldCreateFile(
		string path, List<string> contents)
	{
		string expectedContent = string.Join(Environment.NewLine, contents)
		                         + Environment.NewLine;
		FileSystem.File.AppendAllLines(path, contents);

		await That(FileSystem.File.Exists(path)).IsTrue();
		await That(FileSystem.File.ReadAllText(path)).IsEqualTo(expectedContent);
	}

	[Theory]
	[AutoData]
	public async Task AppendAllLines_NullContent_ShouldThrowArgumentNullException(
		string path)
	{
		void Act()
		{
			FileSystem.File.AppendAllLines(path, null!);
		}

		await That(Act).Throws<ArgumentNullException>()
			.WithHResult(-2147467261).And
			.WithParamName("contents");
	}

	[Theory]
	[AutoData]
	public async Task AppendAllLines_NullEncoding_ShouldThrowArgumentNullException(
		string path)
	{
		void Act()
		{
			FileSystem.File.AppendAllLines(path, new List<string>(), null!);
		}

		await That(Act).Throws<ArgumentNullException>()
			.WithHResult(-2147467261).And
			.WithParamName("encoding");
	}

	[Theory]
	[AutoData]
	public async Task AppendAllLines_ShouldEndWithNewline(string path)
	{
		string[] contents = ["foo", "bar"];
		string expectedResult = "foo" + Environment.NewLine + "bar" + Environment.NewLine;

		FileSystem.File.AppendAllLines(path, contents);

		await That(FileSystem.File.Exists(path)).IsTrue();
		await That(FileSystem.File.ReadAllText(path)).IsEqualTo(expectedResult);
	}

	[Theory]
	[AutoData]
	public async Task
		AppendAllLines_WhenDirectoryWithSameNameExists_ShouldThrowUnauthorizedAccessException(
			string path, string[] contents)
	{
		FileSystem.Directory.CreateDirectory(path);

		void Act()
		{
			FileSystem.File.AppendAllLines(path, contents);
		}

		await That(Act).Throws<UnauthorizedAccessException>().WithHResult(-2147024891);
		await That(FileSystem.Directory.Exists(path)).IsTrue();
		await That(FileSystem.File.Exists(path)).IsFalse();
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

	[Theory]
	[AutoData]
	public async Task AppendAllLines_WithoutEncoding_ShouldUseUtf8(
		string path)
	{
		string[] contents = ["breuß"];

		FileSystem.File.AppendAllLines(path, contents);

		byte[] bytes = FileSystem.File.ReadAllBytes(path);
		await That(bytes.Length).IsEqualTo(6 + Environment.NewLine.Length);
	}
}
