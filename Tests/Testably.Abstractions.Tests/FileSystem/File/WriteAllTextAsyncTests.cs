#if FEATURE_FILESYSTEM_ASYNC
using System.IO;
using System.Text;
using System.Threading;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public partial class WriteAllTextAsyncTests
{
	[Theory]
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

	[Theory]
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

	[Theory]
	[AutoData]
	public async Task WriteAllTextAsync_PreviousFile_ShouldOverwriteFileWithText(
		string path, string contents)
	{
		await FileSystem.File.WriteAllTextAsync(path, "foo", TestContext.Current.CancellationToken);
		await FileSystem.File.WriteAllTextAsync(path, contents,
			TestContext.Current.CancellationToken);

		string result =
			await FileSystem.File.ReadAllTextAsync(path, TestContext.Current.CancellationToken);

		await That(result).IsEqualTo(contents);
	}

	[Theory]
	[AutoData]
	public async Task WriteAllTextAsync_ShouldCreateFileWithText(
		string path, string contents)
	{
		await FileSystem.File.WriteAllTextAsync(path, contents,
			TestContext.Current.CancellationToken);

		string result =
			await FileSystem.File.ReadAllTextAsync(path, TestContext.Current.CancellationToken);

		await That(result).IsEqualTo(contents);
	}

	[Theory]
	[AutoData]
	public async Task WriteAllTextAsync_SpecialCharacters_ShouldReturnSameText(
		string path)
	{
		char[] specialCharacters =
		[
			'Ä',
			'Ö',
			'Ü',
			'ä',
			'ö',
			'ü',
			'ß',
		];
		foreach (char specialCharacter in specialCharacters)
		{
			string contents = "_" + specialCharacter;
			await FileSystem.File.WriteAllTextAsync(path, contents,
				TestContext.Current.CancellationToken);

			string result =
				await FileSystem.File.ReadAllTextAsync(path, TestContext.Current.CancellationToken);

			await That(result).IsEqualTo(contents).Because($"{contents} should be encoded and decoded identical.");
		}
	}

	[Theory]
	[AutoData]
	public async Task WriteAllTextAsync_WhenContentIsNull_ShouldNotThrowException(string path)
	{
		async Task Act()
		{
			await FileSystem.File.WriteAllTextAsync(path, (string?)null,
				TestContext.Current.CancellationToken);
		}

		Exception? exception = await Record.ExceptionAsync(Act);
		await That(exception).IsNull();
	}

	[Theory]
	[AutoData]
	public async Task
		WriteAllTextAsync_WhenDirectoryWithSameNameExists_ShouldThrowUnauthorizedAccessException(
			string path, string contents)
	{
		FileSystem.Directory.CreateDirectory(path);

		async Task Act()
		{
			await FileSystem.File.WriteAllTextAsync(path, contents,
				TestContext.Current.CancellationToken);
		}

		Exception? exception = await Record.ExceptionAsync(Act);

		exception.Should().BeException<UnauthorizedAccessException>(
			hResult: -2147024891);
		await That(FileSystem.Directory.Exists(path)).IsTrue();
		await That(FileSystem.File.Exists(path)).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task
		WriteAllTextAsync_WhenFileIsHidden_ShouldThrowUnauthorizedAccessException_OnWindows(
			string path, string contents)
	{
		Skip.IfNot(Test.RunsOnWindows);

		await FileSystem.File.WriteAllTextAsync(path, "", TestContext.Current.CancellationToken);
		FileSystem.File.SetAttributes(path, FileAttributes.Hidden);

		async Task Act()
		{
			await FileSystem.File.WriteAllTextAsync(path, contents,
				TestContext.Current.CancellationToken);
		}

		Exception? exception = await Record.ExceptionAsync(Act);

		exception.Should().BeException<UnauthorizedAccessException>(hResult: -2147024891);
	}

#if FEATURE_FILE_SPAN
	[Theory]
	[AutoData]
	public async Task WriteAllTextAsync_ReadOnlyMemory_Cancelled_ShouldThrowTaskCanceledException(
		string path, string? contents)
	{
		using CancellationTokenSource cts = new();
		await cts.CancelAsync();

		Exception? exception = await Record.ExceptionAsync(() =>
			FileSystem.File.WriteAllTextAsync(path, contents.AsMemory(), cts.Token));

		exception.Should().BeException<TaskCanceledException>(hResult: -2146233029);
	}

	[Theory]
	[AutoData]
	public async Task
		WriteAllTextAsync_ReadOnlyMemory_Cancelled_WithEncoding_ShouldThrowTaskCanceledException(
			string path, string? contents)
	{
		using CancellationTokenSource cts = new();
		await cts.CancelAsync();

		Exception? exception = await Record.ExceptionAsync(() =>
			FileSystem.File.WriteAllTextAsync(path, contents.AsMemory(), Encoding.UTF8, cts.Token));

		exception.Should().BeException<TaskCanceledException>(hResult: -2146233029);
	}

	[Theory]
	[AutoData]
	public async Task WriteAllTextAsync_ReadOnlyMemory_PreviousFile_ShouldOverwriteFileWithText(
		string path, string contents)
	{
		await FileSystem.File.WriteAllTextAsync(path, "foo", TestContext.Current.CancellationToken);
		await FileSystem.File.WriteAllTextAsync(path, contents.AsMemory(),
			TestContext.Current.CancellationToken);

		string result =
			await FileSystem.File.ReadAllTextAsync(path, TestContext.Current.CancellationToken);

		await That(result).IsEqualTo(contents);
	}

	[Theory]
	[AutoData]
	public async Task WriteAllTextAsync_ReadOnlyMemory_ShouldCreateFileWithText(
		string path, string contents)
	{
		await FileSystem.File.WriteAllTextAsync(path, contents.AsMemory(),
			TestContext.Current.CancellationToken);

		string result =
			await FileSystem.File.ReadAllTextAsync(path, TestContext.Current.CancellationToken);

		await That(result).IsEqualTo(contents);
	}

	[Theory]
	[AutoData]
	public async Task WriteAllTextAsync_ReadOnlyMemory_SpecialCharacters_ShouldReturnSameText(
		string path)
	{
		char[] specialCharacters =
		[
			'Ä',
			'Ö',
			'Ü',
			'ä',
			'ö',
			'ü',
			'ß',
		];
		foreach (char specialCharacter in specialCharacters)
		{
			string contents = "_" + specialCharacter;
			await FileSystem.File.WriteAllTextAsync(path, contents.AsMemory(),
				TestContext.Current.CancellationToken);

			string result =
				await FileSystem.File.ReadAllTextAsync(path, TestContext.Current.CancellationToken);

			await That(result).IsEqualTo(contents).Because($"{contents} should be encoded and decoded identical.");
		}
	}

	[Theory]
	[AutoData]
	public async Task
		WriteAllTextAsync_ReadOnlyMemory_WhenDirectoryWithSameNameExists_ShouldThrowUnauthorizedAccessException(
			string path, string contents)
	{
		FileSystem.Directory.CreateDirectory(path);

		async Task Act()
		{
			await FileSystem.File.WriteAllTextAsync(path, contents.AsMemory(),
				TestContext.Current.CancellationToken);
		}

		Exception? exception = await Record.ExceptionAsync(Act);

		exception.Should().BeException<UnauthorizedAccessException>(
			hResult: -2147024891);
		await That(FileSystem.Directory.Exists(path)).IsTrue();
		await That(FileSystem.File.Exists(path)).IsFalse();
	}

	[Theory]
	[AutoData]
	public async Task
		WriteAllTextAsync_ReadOnlyMemory_WhenFileIsHidden_ShouldThrowUnauthorizedAccessException_OnWindows(
			string path, string contents)
	{
		Skip.IfNot(Test.RunsOnWindows);

		await FileSystem.File.WriteAllTextAsync(path, "", TestContext.Current.CancellationToken);
		FileSystem.File.SetAttributes(path, FileAttributes.Hidden);

		async Task Act()
		{
			await FileSystem.File.WriteAllTextAsync(path, contents.AsMemory(),
				TestContext.Current.CancellationToken);
		}

		Exception? exception = await Record.ExceptionAsync(Act);

		exception.Should().BeException<UnauthorizedAccessException>(hResult: -2147024891);
	}
#endif
}
#endif
