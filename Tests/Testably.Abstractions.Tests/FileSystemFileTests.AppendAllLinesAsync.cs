#if FEATURE_FILESYSTEM_ASYNC
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemFileTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    [Theory]
    [AutoData]
    public async Task AppendAllLinesAsync_ExistingFile_ShouldAppendLinesToFile(
        string path, List<string> previousContents, List<string> contents)
    {
        await FileSystem.File.AppendAllLinesAsync(path, previousContents);

        await FileSystem.File.AppendAllLinesAsync(path, contents);

        FileSystem.File.Exists(path).Should().BeTrue();
        FileSystem.File.ReadAllLines(path).Should()
           .BeEquivalentTo(previousContents.Concat(contents));
    }

    [Theory]
    [AutoData]
    public async Task AppendAllLinesAsync_MissingFile_ShouldCreateFile(
        string path, List<string> contents)
    {
        await FileSystem.File.AppendAllLinesAsync(path, contents);

        FileSystem.File.Exists(path).Should().BeTrue();
        FileSystem.File.ReadAllLines(path).Should().BeEquivalentTo(contents);
    }

    [Theory]
    [AutoData]
    public async Task AppendAllLinesAsync_ShouldEndWithNewline(string path)
    {
        string[] contents = { "foo", "bar" };
        string expectedResult = "foo" + Environment.NewLine + "bar" + Environment.NewLine;

        await FileSystem.File.AppendAllLinesAsync(path, contents);

        FileSystem.File.ReadAllText(path).Should().BeEquivalentTo(expectedResult);
    }

    [Theory]
    [MemberAutoData(nameof(GetEncodingDifference))]
    public async Task AppendAllLinesAsync_WithDifferentEncoding_ShouldNotReturnWrittenText(
        string specialLine, Encoding writeEncoding, Encoding readEncoding,
        string path, string[] contents)
    {
        contents[1] = specialLine;
        await FileSystem.File.AppendAllLinesAsync(path, contents, writeEncoding);

        string[] result = FileSystem.File.ReadAllLines(path, readEncoding);

        result.Should().NotBeEquivalentTo(contents,
            $"{contents} should be different when encoding from {writeEncoding} to {readEncoding}.");
        result[0].Should().Be(contents[0]);
    }
}
#endif