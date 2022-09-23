using System.IO;
using System.Text;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemFileTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    [Theory]
    [AutoData]
    public void ReadAllLines_MissingFile_ShouldThrowFileNotFoundException(string path)
    {
        Exception? exception = Record.Exception(() =>
        {
            _ = FileSystem.File.ReadAllLines(path);
        });

        exception.Should().BeOfType<FileNotFoundException>()
           .Which.Message.Should()
           .Be($"Could not find file '{FileSystem.Path.GetFullPath(path)}'.");
    }

    [Theory]
    [AutoData]
    public void ReadAllLines_ShouldEnumerateLines(string path, string[] lines)
    {
        string contents = string.Join(Environment.NewLine, lines);
        FileSystem.File.WriteAllText(path, contents);

        string[] results = FileSystem.File.ReadAllLines(path);

        results.Should().BeEquivalentTo(lines);
    }

    [Theory]
    [MemberAutoData(nameof(GetEncodingDifference))]
    public void ReadAllLines_WithDifferentEncoding_ShouldNotReturnWrittenText(
        string specialLine, Encoding writeEncoding, Encoding readEncoding,
        string path, string[] lines)
    {
        lines[1] = specialLine;
        string contents = string.Join(Environment.NewLine, lines);
        FileSystem.File.WriteAllText(path, contents, writeEncoding);

        string[] result = FileSystem.File.ReadAllLines(path, readEncoding);

        result.Should().NotBeEquivalentTo(lines,
            $"{contents} should be different when encoding from {writeEncoding} to {readEncoding}.");
        result[0].Should().Be(lines[0]);
    }
}