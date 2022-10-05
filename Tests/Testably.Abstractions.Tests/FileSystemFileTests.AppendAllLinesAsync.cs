#if FEATURE_FILESYSTEM_ASYNC
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests;

// ReSharper disable MethodHasAsyncOverload
public abstract partial class FileSystemFileTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    [SkippableTheory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.AppendAllLinesAsync))]
    public async Task AppendAllLinesAsync_Cancelled_ShouldThrowTaskCanceledException(
        string path, List<string> contents)
    {
        CancellationTokenSource cts = new();
        cts.Cancel();

        Exception? exception = await Record.ExceptionAsync(() =>
            FileSystem.File.AppendAllLinesAsync(path, contents, cts.Token));

        exception.Should().BeOfType<TaskCanceledException>()
           .Which.Message.Should().Be("A task was canceled.");
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.AppendAllLinesAsync))]
    public async Task
        AppendAllLinesAsync_Cancelled_WithEncoding_ShouldThrowTaskCanceledException(
            string path, List<string> contents)
    {
        CancellationTokenSource cts = new();
        cts.Cancel();

        Exception? exception = await Record.ExceptionAsync(() =>
            FileSystem.File.AppendAllLinesAsync(path, contents, Encoding.UTF8,
                cts.Token));

        exception.Should().BeOfType<TaskCanceledException>()
           .Which.Message.Should().Be("A task was canceled.");
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.AppendAllLinesAsync))]
    public async Task AppendAllLinesAsync_ExistingFile_ShouldAppendLinesToFile(
        string path, List<string> previousContents, List<string> contents)
    {
        await FileSystem.File.AppendAllLinesAsync(path, previousContents);

        await FileSystem.File.AppendAllLinesAsync(path, contents);

        FileSystem.File.Exists(path).Should().BeTrue();
        FileSystem.File.ReadAllLines(path).Should()
           .BeEquivalentTo(previousContents.Concat(contents));
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.AppendAllLinesAsync))]
    public async Task AppendAllLinesAsync_MissingFile_ShouldCreateFile(
        string path, List<string> contents)
    {
        await FileSystem.File.AppendAllLinesAsync(path, contents);

        FileSystem.File.Exists(path).Should().BeTrue();
        FileSystem.File.ReadAllLines(path).Should().BeEquivalentTo(contents);
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.AppendAllLinesAsync))]
    public async Task AppendAllLinesAsync_ShouldEndWithNewline(string path)
    {
        string[] contents = { "foo", "bar" };
        string expectedResult = "foo" + Environment.NewLine + "bar" + Environment.NewLine;

        await FileSystem.File.AppendAllLinesAsync(path, contents);

        FileSystem.File.ReadAllText(path).Should().BeEquivalentTo(expectedResult);
    }

    [SkippableTheory]
    [MemberAutoData(nameof(GetEncodingDifference))]
    [FileSystemTests.File(nameof(IFileSystem.IFile.AppendAllLinesAsync))]
    public async Task
        AppendAllLinesAsync_WithDifferentEncoding_ShouldNotReturnWrittenText(
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