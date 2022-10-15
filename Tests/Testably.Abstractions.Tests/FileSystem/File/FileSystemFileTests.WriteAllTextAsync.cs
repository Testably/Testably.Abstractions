#if FEATURE_FILESYSTEM_ASYNC
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests.FileSystem.File;

public abstract partial class FileSystemFileTests<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public async Task WriteAllTextAsync_Cancelled_ShouldThrowTaskCanceledException(
		string path, string? contents)
	{
		CancellationTokenSource cts = new();
		cts.Cancel();

		Exception? exception = await Record.ExceptionAsync(() =>
			FileSystem.File.WriteAllTextAsync(path, contents, cts.Token));

		exception.Should().BeOfType<TaskCanceledException>()
		   .Which.Message.Should().Be("A task was canceled.");
	}

	[SkippableTheory]
	[AutoData]
	public async Task
		WriteAllTextAsync_Cancelled_WithEncoding_ShouldThrowTaskCanceledException(
			string path, string? contents)
	{
		CancellationTokenSource cts = new();
		cts.Cancel();

		Exception? exception = await Record.ExceptionAsync(() =>
			FileSystem.File.WriteAllTextAsync(path, contents, Encoding.UTF8, cts.Token));

		exception.Should().BeOfType<TaskCanceledException>()
		   .Which.Message.Should().Be("A task was canceled.");
	}

	[SkippableTheory]
	[AutoData]
	public async Task WriteAllTextAsync_PreviousFile_ShouldOverwriteFileWithText(
		string path, string contents)
	{
		await FileSystem.File.WriteAllTextAsync(path, "foo");
		await FileSystem.File.WriteAllTextAsync(path, contents);

		string result = await FileSystem.File.ReadAllTextAsync(path);

		result.Should().Be(contents);
	}

	[SkippableTheory]
	[AutoData]
	public async Task WriteAllTextAsync_ShouldCreateFileWithText(
		string path, string contents)
	{
		await FileSystem.File.WriteAllTextAsync(path, contents);

		string result = await FileSystem.File.ReadAllTextAsync(path);

		result.Should().Be(contents);
	}

	[SkippableTheory]
	[AutoData]
	public async Task WriteAllTextAsync_SpecialCharacters_ShouldReturnSameText(
		string path)
	{
		char[] specialCharacters = { 'Ä', 'Ö', 'Ü', 'ä', 'ö', 'ü', 'ß' };
		foreach (char specialCharacter in specialCharacters)
		{
			string contents = "_" + specialCharacter;
			await FileSystem.File.WriteAllTextAsync(path, contents);

			string result = await FileSystem.File.ReadAllTextAsync(path);

			result.Should().Be(contents,
				$"{contents} should be encoded and decoded identical.");
		}
	}
}
#endif