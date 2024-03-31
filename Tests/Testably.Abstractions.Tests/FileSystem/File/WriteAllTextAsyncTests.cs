#if FEATURE_FILESYSTEM_ASYNC
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests.FileSystem.File;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class WriteAllTextAsyncTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public async Task WriteAllTextAsync_Cancelled_ShouldThrowTaskCanceledException(
		string path, string? contents)
	{
		using CancellationTokenSource cts = new();
		await cts.CancelAsync();

		Exception? exception = await Record.ExceptionAsync(() =>
			FileSystem.File.WriteAllTextAsync(path, contents, cts.Token));

		exception.Should().BeException<TaskCanceledException>(hResult: -2146233029);
	}

	[SkippableTheory]
	[AutoData]
	public async Task
		WriteAllTextAsync_Cancelled_WithEncoding_ShouldThrowTaskCanceledException(
			string path, string? contents)
	{
		using CancellationTokenSource cts = new();
		await cts.CancelAsync();

		Exception? exception = await Record.ExceptionAsync(() =>
			FileSystem.File.WriteAllTextAsync(path, contents, Encoding.UTF8, cts.Token));

		exception.Should().BeException<TaskCanceledException>(hResult: -2146233029);
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
		char[] specialCharacters =
		{
			'Ä',
			'Ö',
			'Ü',
			'ä',
			'ö',
			'ü',
			'ß'
		};
		foreach (char specialCharacter in specialCharacters)
		{
			string contents = "_" + specialCharacter;
			await FileSystem.File.WriteAllTextAsync(path, contents);

			string result = await FileSystem.File.ReadAllTextAsync(path);

			result.Should().Be(contents,
				$"{contents} should be encoded and decoded identical.");
		}
	}

	[SkippableTheory]
	[AutoData]
	public async Task WriteAllTextAsync_WhenContentIsNull_ShouldNotThrowException(string path)
	{
		Exception? exception = await Record.ExceptionAsync(async () =>
		{
			await FileSystem.File.WriteAllTextAsync(path, null);
		});

		exception.Should().BeNull();
	}

	[SkippableTheory]
	[AutoData]
	public async Task
		WriteAllTextAsync_WhenDirectoryWithSameNameExists_ShouldThrowUnauthorizedAccessException(
			string path, string contents)
	{
		FileSystem.Directory.CreateDirectory(path);

		Exception? exception = await Record.ExceptionAsync(async () =>
		{
			await FileSystem.File.WriteAllTextAsync(path, contents);
		});

		exception.Should().BeException<UnauthorizedAccessException>(
			hResult: -2147024891);
		FileSystem.Should().HaveDirectory(path);
		FileSystem.Should().NotHaveFile(path);
	}

	[SkippableTheory]
	[AutoData]
	public async Task
		WriteAllTextAsync_WhenFileIsHidden_ShouldThrowUnauthorizedAccessException_OnWindows(
			string path, string contents)
	{
		Skip.IfNot(Test.RunsOnWindows);

		await FileSystem.File.WriteAllTextAsync(path, null);
		FileSystem.File.SetAttributes(path, FileAttributes.Hidden);

		Exception? exception = await Record.ExceptionAsync(async () =>
		{
			await FileSystem.File.WriteAllTextAsync(path, contents);
		});

		exception.Should().BeException<UnauthorizedAccessException>(hResult: -2147024891);
	}
}
#endif
