#if FEATURE_FILESYSTEM_ASYNC
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemFileTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    [SkippableTheory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.ReadAllBytesAsync))]
    public async Task ReadAllBytesAsync_Cancelled_ShouldThrowTaskCanceledException(
        string path)
    {
        CancellationTokenSource cts = new();
        cts.Cancel();

        Exception? exception = await Record.ExceptionAsync(() =>
            FileSystem.File.ReadAllBytesAsync(path, cts.Token));

        exception.Should().BeOfType<TaskCanceledException>()
           .Which.Message.Should().Be("A task was canceled.");
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.ReadAllBytesAsync))]
    public async Task ReadAllBytesAsync_MissingFile_ShouldThrowFileNotFoundException(
        string path)
    {
        Exception? exception = await Record.ExceptionAsync(() =>
            FileSystem.File.ReadAllBytesAsync(path));

        exception.Should().BeOfType<FileNotFoundException>()
           .Which.Message.Should()
           .Contain($"'{FileSystem.Path.GetFullPath(path)}'");
    }

    [SkippableTheory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.ReadAllBytesAsync))]
    public async Task ReadAllBytesAsync_ShouldReturnWrittenBytes(
        byte[] contents, string path)
    {
        await FileSystem.File.WriteAllBytesAsync(path, contents);

        byte[] result = await FileSystem.File.ReadAllBytesAsync(path);

        result.Should().BeEquivalentTo(contents);
    }
}
#endif