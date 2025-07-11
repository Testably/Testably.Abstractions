using AutoFixture;
using System.IO;
using System.Linq;
using System.Text;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class ReadLinesTests
{
	[Theory]
	[AutoData]
	public async Task ReadLines_EmptyFile_ShouldEnumerateLines(string path)
	{
		FileSystem.File.WriteAllText(path, null);

		string[] results = FileSystem.File.ReadLines(path).ToArray();

		await That(results).IsEmpty();
	}

	[Theory]
	[AutoData]
	public async Task ReadLines_MissingFile_ShouldThrowFileNotFoundException(string path)
	{
		void Act()
		{
			_ = FileSystem.File.ReadLines(path).FirstOrDefault();
		}

		await That(Act).Throws<FileNotFoundException>()
			.WithMessageContaining($"'{FileSystem.Path.GetFullPath(path)}'").And
			.WithHResult(-2147024894);
	}

	[Theory]
	[AutoData]
	public async Task ReadLines_ShouldEnumerateLines(string path, string[] lines)
	{
		string contents = string.Join(Environment.NewLine, lines);
		FileSystem.File.WriteAllText(path, contents);

		string[] results = FileSystem.File.ReadLines(path).ToArray();

		await That(results).IsEqualTo(lines).InAnyOrder();
	}

	[Theory]
	[ClassData(typeof(TestDataGetEncodingDifference))]
	public async Task ReadLines_WithDifferentEncoding_ShouldNotReturnWrittenText(
		string specialLine, Encoding writeEncoding, Encoding readEncoding)
	{
		string path = new Fixture().Create<string>();
		string[] lines = new Fixture().Create<string[]>();
		lines[1] = specialLine;
		string contents = string.Join(Environment.NewLine, lines);
		FileSystem.File.WriteAllText(path, contents, writeEncoding);

		string[] result = FileSystem.File.ReadLines(path, readEncoding).ToArray();

		await That(result).IsNotEqualTo(lines).InAnyOrder();
		await That(result[0]).IsEqualTo(lines[0]);
	}
}
