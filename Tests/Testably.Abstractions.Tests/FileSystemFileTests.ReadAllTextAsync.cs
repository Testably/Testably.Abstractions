#if FEATURE_FILESYSTEM_ASYNC
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests;

public abstract partial class FileSystemFileTests<TFileSystem>
    where TFileSystem : IFileSystem
{
    [Theory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.ReadAllTextAsync))]
    public async Task ReadAllTextAsync_Cancelled_ShouldThrowTaskCanceledException(
        string path)
    {
        CancellationTokenSource cts = new();
        cts.Cancel();

        Exception? exception = await Record.ExceptionAsync(() =>
            FileSystem.File.ReadAllTextAsync(path, cts.Token));

        exception.Should().BeOfType<TaskCanceledException>()
           .Which.Message.Should().Be("A task was canceled.");
    }

    [Theory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.ReadAllTextAsync))]
    public async Task
        ReadAllTextAsync_Cancelled_WithEncoding_ShouldThrowTaskCanceledException(
            string path)
    {
        CancellationTokenSource cts = new();
        cts.Cancel();

        Exception? exception = await Record.ExceptionAsync(() =>
            FileSystem.File.ReadAllTextAsync(path, Encoding.UTF8, cts.Token));

        exception.Should().BeOfType<TaskCanceledException>()
           .Which.Message.Should().Be("A task was canceled.");
    }

    [Theory]
    [AutoData]
    [FileSystemTests.File(nameof(IFileSystem.IFile.ReadAllTextAsync))]
    public async Task ReadAllTextAsync_MissingFile_ShouldThrowFileNotFoundException(
        string path)
    {
        Exception? exception = await Record.ExceptionAsync(() =>
            FileSystem.File.ReadAllTextAsync(path));

        exception.Should().BeOfType<FileNotFoundException>()
           .Which.Message.Should()
           .Contain($"'{FileSystem.Path.GetFullPath(path)}'");
    }

    [Theory]
    [MemberAutoData(nameof(GetEncodingDifference))]
    [FileSystemTests.File(nameof(IFileSystem.IFile.ReadAllTextAsync))]
    public async Task ReadAllTextAsync_WithDifferentEncoding_ShouldNotReturnWrittenText(
        string contents, Encoding writeEncoding, Encoding readEncoding, string path)
    {
        await FileSystem.File.WriteAllTextAsync(path, contents, writeEncoding);

        string result = await FileSystem.File.ReadAllTextAsync(path, readEncoding);

        result.Should().NotBe(contents,
            $"{contents} should be different when encoding from {writeEncoding} to {readEncoding}.");
    }
}
#endif