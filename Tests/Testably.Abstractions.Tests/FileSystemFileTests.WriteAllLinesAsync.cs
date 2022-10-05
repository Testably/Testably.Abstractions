#if FEATURE_FILESYSTEM_ASYNC
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemFileTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    [SkippableTheory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.WriteAllLinesAsync))]
    public async Task WriteAllLinesAsync_Cancelled_ShouldThrowTaskCanceledException(
        string path, string[] contents)
    {
        CancellationTokenSource cts = new();
        cts.Cancel();

        Exception? exception = await Record.ExceptionAsync(() =>
            FileSystem.File.WriteAllLinesAsync(path, contents, cts.Token));

        exception.Should().BeOfType<TaskCanceledException>()
           .Which.Message.Should().Be("A task was canceled.");
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.WriteAllLinesAsync))]
    public async Task
        WriteAllLinesAsync_Cancelled_WithEncoding_ShouldThrowTaskCanceledException(
            string path, string[] contents)
    {
        CancellationTokenSource cts = new();
        cts.Cancel();

        Exception? exception = await Record.ExceptionAsync(() =>
            FileSystem.File.WriteAllLinesAsync(path, contents, Encoding.UTF8, cts.Token));

        exception.Should().BeOfType<TaskCanceledException>()
           .Which.Message.Should().Be("A task was canceled.");
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.WriteAllLinesAsync))]
    public async Task WriteAllLinesAsync_PreviousFile_ShouldOverwriteFileWithText(
        string path, string[] contents)
    {
        await FileSystem.File.WriteAllTextAsync(path, "foo");

        await FileSystem.File.WriteAllLinesAsync(path, contents);

        string[] result = await FileSystem.File.ReadAllLinesAsync(path);
        result.Should().BeEquivalentTo(contents);
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.WriteAllLinesAsync))]
    public async Task WriteAllLinesAsync_ShouldCreateFileWithText(
        string path, string[] contents)
    {
        await FileSystem.File.WriteAllLinesAsync(path, contents);

        string[] result = await FileSystem.File.ReadAllLinesAsync(path);
        result.Should().BeEquivalentTo(contents);
    }
}
#endif