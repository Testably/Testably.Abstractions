using AutoFixture;
using System.IO;
using System.Text;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class ReadAllLinesTests
{
	[Theory]
	[AutoData]
	public async Task ReadAllLines_Empty_ShouldReturnEmptyArray(string path)
	{
		FileSystem.File.WriteAllText(path, "");

		string[] results = FileSystem.File.ReadAllLines(path);

		await That(results).IsEmpty();
	}

	[Theory]
	[AutoData]
	public async Task ReadAllLines_MissingFile_ShouldThrowFileNotFoundException(string path)
	{
		void Act()
		{
			_ = FileSystem.File.ReadAllLines(path);
		}

		await That(Act).Throws<FileNotFoundException>()
			.WithMessageContaining($"'{FileSystem.Path.GetFullPath(path)}'").And
			.WithHResult(-2147024894);
	}

	[Theory]
	[AutoData]
	public async Task ReadAllLines_ShouldEnumerateLines(string path, string[] lines)
	{
		string contents = string.Join(Environment.NewLine, lines);
		FileSystem.File.WriteAllText(path, contents);

		string[] results = FileSystem.File.ReadAllLines(path);

		await That(results).IsEqualTo(lines);
	}

	[Theory]
	[AutoData]
	public async Task ReadAllLines_ShouldNotReturnByteOrderMark(string path, string content)
	{
		FileSystem.File.WriteAllLines(path, [content], Encoding.UTF32);

		string[] result = FileSystem.File.ReadAllLines(path, Encoding.UTF32);

		//Ensure that the file contains the BOM-Bytes
		await That(FileSystem.File.ReadAllBytes(path).Length).IsGreaterThan(content.Length);
		await That(result.Length).IsEqualTo(1);
		await That(result[0]).IsEqualTo(content);
	}

	[Theory]
	[ClassData(typeof(TestDataGetEncodingDifference))]
	public async Task ReadAllLines_WithDifferentEncoding_ShouldNotReturnWrittenText(
		string specialLine, Encoding writeEncoding, Encoding readEncoding)
	{
		string path = new Fixture().Create<string>();
		string[] lines = new Fixture().Create<string[]>();
		lines[1] = specialLine;
		string contents = string.Join(Environment.NewLine, lines);
		FileSystem.File.WriteAllText(path, contents, writeEncoding);

		string[] result = FileSystem.File.ReadAllLines(path, readEncoding);

		await That(result).IsNotEqualTo(lines).InAnyOrder();
		await That(result[0]).IsEqualTo(lines[0]);
	}
}
