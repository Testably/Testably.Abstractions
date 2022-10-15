#if FEATURE_FILESYSTEM_ASYNC
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests.FileSystem.File;

// ReSharper disable MethodHasAsyncOverload
public abstract partial class FileSystemFileTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	[FileSystemTests.File(nameof(IFileSystem.IFile.AppendAllTextAsync))]
	public async Task AppendAllTextAsync_Cancelled_ShouldThrowTaskCanceledException(
		string path, string contents)
	{
		CancellationTokenSource cts = new();
		cts.Cancel();

		Exception? exception = await Record.ExceptionAsync(() =>
			FileSystem.File.AppendAllTextAsync(path, contents, cts.Token));

		exception.Should().BeOfType<TaskCanceledException>()
		   .Which.Message.Should().Be("A task was canceled.");
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.File(nameof(IFileSystem.IFile.AppendAllTextAsync))]
	public async Task
		AppendAllTextAsync_Cancelled_WithEncoding_ShouldThrowTaskCanceledException(
			string path, string contents)
	{
		CancellationTokenSource cts = new();
		cts.Cancel();

		Exception? exception = await Record.ExceptionAsync(() =>
			FileSystem.File.AppendAllTextAsync(path, contents, Encoding.UTF8, cts.Token));

		exception.Should().BeOfType<TaskCanceledException>()
		   .Which.Message.Should().Be("A task was canceled.");
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.File(nameof(IFileSystem.IFile.AppendAllTextAsync))]
	public async Task AppendAllTextAsync_ExistingFile_ShouldAppendLinesToFile(
		string path, string previousContents, string contents)
	{
		await FileSystem.File.AppendAllTextAsync(path, previousContents);

		await FileSystem.File.AppendAllTextAsync(path, contents);

		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.ReadAllText(path).Should()
		   .BeEquivalentTo(previousContents + contents);
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.File(nameof(IFileSystem.IFile.AppendAllTextAsync))]
	public async Task AppendAllTextAsync_MissingFile_ShouldCreateFile(
		string path, string contents)
	{
		await FileSystem.File.AppendAllTextAsync(path, contents);

		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.ReadAllLines(path).Should().BeEquivalentTo(contents);
	}

	[SkippableTheory]
	[AutoData]
	[FileSystemTests.File(nameof(IFileSystem.IFile.AppendAllTextAsync))]
	public async Task AppendAllTextAsync_ShouldNotEndWithNewline(string path)
	{
		string contents = "foo";

		await FileSystem.File.AppendAllTextAsync(path, contents);

		FileSystem.File.ReadAllText(path).Should().BeEquivalentTo(contents);
	}

	[SkippableTheory]
	[MemberAutoData(nameof(GetEncodingDifference))]
	[FileSystemTests.File(nameof(IFileSystem.IFile.AppendAllTextAsync))]
	public async Task AppendAllTextAsync_WithDifferentEncoding_ShouldNotReturnWrittenText(
		string contents, Encoding writeEncoding, Encoding readEncoding,
		string path)
	{
		await FileSystem.File.AppendAllTextAsync(path, contents, writeEncoding);

		string[] result = FileSystem.File.ReadAllLines(path, readEncoding);

		result.Should().NotBeEquivalentTo(contents,
			$"{contents} should be different when encoding from {writeEncoding} to {readEncoding}.");
	}
}
#endif