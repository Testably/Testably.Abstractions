using AutoFixture;
using System.IO;
using System.Linq;
using System.Text;

namespace Testably.Abstractions.Tests.FileSystem.File;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class ReadLinesTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public void ReadLines_EmptyFile_ShouldEnumerateLines(string path)
	{
		FileSystem.File.WriteAllText(path, null);

		string[] results = FileSystem.File.ReadLines(path).ToArray();

		results.Should().BeEmpty();
	}

	[SkippableTheory]
	[AutoData]
	public void ReadLines_MissingFile_ShouldThrowFileNotFoundException(string path)
	{
		Exception? exception = Record.Exception(() =>
		{
			_ = FileSystem.File.ReadLines(path).FirstOrDefault();
		});

		exception.Should().BeOfType<FileNotFoundException>()
		   .Which.HResult.Should().Be(-2147024894);
		exception.Should().BeOfType<FileNotFoundException>()
		   .Which.Message.Should()
		   .Contain($"'{FileSystem.Path.GetFullPath(path)}'");
	}

	[SkippableTheory]
	[AutoData]
	public void ReadLines_ShouldEnumerateLines(string path, string[] lines)
	{
		string contents = string.Join(Environment.NewLine, lines);
		FileSystem.File.WriteAllText(path, contents);

		string[] results = FileSystem.File.ReadLines(path).ToArray();

		results.Should().BeEquivalentTo(lines);
	}

	[SkippableTheory]
	[ClassData(typeof(TestDataGetEncodingDifference))]
	public void ReadLines_WithDifferentEncoding_ShouldNotReturnWrittenText(
		string specialLine, Encoding writeEncoding, Encoding readEncoding)
	{
		string path = new Fixture().Create<string>();
		string[] lines = new Fixture().Create<string[]>();
		lines[1] = specialLine;
		string contents = string.Join(Environment.NewLine, lines);
		FileSystem.File.WriteAllText(path, contents, writeEncoding);

		string[] result = FileSystem.File.ReadLines(path, readEncoding).ToArray();

		result.Should().NotBeEquivalentTo(lines,
			$"{contents} should be different when encoding from {writeEncoding} to {readEncoding}.");
		result[0].Should().Be(lines[0]);
	}
}