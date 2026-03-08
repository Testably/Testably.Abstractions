#if FEATURE_FILESYSTEM_ASYNC
using System.IO;
using System.Text;
using System.Threading;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public class WriteAllTextAsyncTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	[AutoArguments]
	public async Task WriteAllTextAsync_Cancelled_ShouldThrowTaskCanceledException(
		string path, string? contents)
	{
		using CancellationTokenSource cts = new();
		await cts.CancelAsync();

		async Task Act() =>
			await FileSystem.File.WriteAllTextAsync(path, contents, cts.Token);

		await That(Act).Throws<TaskCanceledException>().WithHResult(-2146233029);
	}

	[Test]
	[AutoArguments]
	public async Task
		WriteAllTextAsync_Cancelled_WithEncoding_ShouldThrowTaskCanceledException(
			string path, string? contents)
	{
		using CancellationTokenSource cts = new();
		await cts.CancelAsync();

		async Task Act() =>
			await FileSystem.File.WriteAllTextAsync(path, contents, Encoding.UTF8, cts.Token);

		await That(Act).Throws<TaskCanceledException>().WithHResult(-2146233029);
	}

	[Test]
	[AutoArguments]
	public async Task WriteAllTextAsync_PreviousFile_ShouldOverwriteFileWithText(
		string path, string contents)
	{
		await FileSystem.File.WriteAllTextAsync(path, "foo", CancellationToken);
		await FileSystem.File.WriteAllTextAsync(path, contents,
			CancellationToken);

		string result =
			await FileSystem.File.ReadAllTextAsync(path, CancellationToken);

		await That(result).IsEqualTo(contents);
	}

	[Test]
	[AutoArguments]
	public async Task WriteAllTextAsync_ShouldCreateFileWithText(
		string path, string contents)
	{
		await FileSystem.File.WriteAllTextAsync(path, contents,
			CancellationToken);

		string result =
			await FileSystem.File.ReadAllTextAsync(path, CancellationToken);

		await That(result).IsEqualTo(contents);
	}

	[Test]
	[AutoArguments]
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
				CancellationToken);

			string result =
				await FileSystem.File.ReadAllTextAsync(path, CancellationToken);

			await That(result).IsEqualTo(contents)
				.Because($"{contents} should be encoded and decoded identical.");
		}
	}

	[Test]
	[AutoArguments]
	public async Task WriteAllTextAsync_WhenContentIsNull_ShouldNotThrowException(string path)
	{
		async Task Act()
		{
			await FileSystem.File.WriteAllTextAsync(path, (string?)null,
				CancellationToken);
		}

		await That(Act).DoesNotThrow();
	}

	[Test]
	[AutoArguments]
	public async Task
		WriteAllTextAsync_WhenDirectoryWithSameNameExists_ShouldThrowUnauthorizedAccessException(
			string path, string contents)
	{
		FileSystem.Directory.CreateDirectory(path);

		async Task Act()
		{
			await FileSystem.File.WriteAllTextAsync(path, contents,
				CancellationToken);
		}

		await That(Act).Throws<UnauthorizedAccessException>().WithHResult(-2147024891);
		await That(FileSystem.Directory.Exists(path)).IsTrue();
		await That(FileSystem.File.Exists(path)).IsFalse();
	}

	[Test]
	[AutoArguments]
	public async Task
		WriteAllTextAsync_WhenFileIsHidden_ShouldThrowUnauthorizedAccessException_OnWindows(
			string path, string contents)
	{
		Skip.IfNot(Test.RunsOnWindows);

		await FileSystem.File.WriteAllTextAsync(path, "", CancellationToken);
		FileSystem.File.SetAttributes(path, FileAttributes.Hidden);

		async Task Act()
		{
			await FileSystem.File.WriteAllTextAsync(path, contents,
				CancellationToken);
		}

		await That(Act).Throws<UnauthorizedAccessException>().WithHResult(-2147024891);
	}

	[Test]
	[AutoArguments]
	public async Task WriteAllTextAsync_WithoutEncoding_ShouldUseUtf8(
		string path)
	{
		string contents = "breuß";

		await FileSystem.File.WriteAllTextAsync(path, contents, CancellationToken.None);

		byte[] bytes = FileSystem.File.ReadAllBytes(path);
		await That(bytes.Length).IsEqualTo(6);
	}

#if FEATURE_FILE_SPAN
	[Test]
	[AutoArguments]
	public async Task WriteAllTextAsync_ReadOnlyMemory_Cancelled_ShouldThrowTaskCanceledException(
		string path, string? contents)
	{
		using CancellationTokenSource cts = new();
		await cts.CancelAsync();

		async Task Act() =>
			await FileSystem.File.WriteAllTextAsync(path, contents.AsMemory(), cts.Token);

		await That(Act).Throws<TaskCanceledException>().WithHResult(-2146233029);
	}

	[Test]
	[AutoArguments]
	public async Task
		WriteAllTextAsync_ReadOnlyMemory_Cancelled_WithEncoding_ShouldThrowTaskCanceledException(
			string path, string? contents)
	{
		using CancellationTokenSource cts = new();
		await cts.CancelAsync();

		async Task Act() =>
			await FileSystem.File.WriteAllTextAsync(path, contents.AsMemory(), Encoding.UTF8,
				cts.Token);

		await That(Act).Throws<TaskCanceledException>().WithHResult(-2146233029);
	}

	[Test]
	[AutoArguments]
	public async Task WriteAllTextAsync_ReadOnlyMemory_PreviousFile_ShouldOverwriteFileWithText(
		string path, string contents)
	{
		await FileSystem.File.WriteAllTextAsync(path, "foo", CancellationToken);
		await FileSystem.File.WriteAllTextAsync(path, contents.AsMemory(),
			CancellationToken);

		string result =
			await FileSystem.File.ReadAllTextAsync(path, CancellationToken);

		await That(result).IsEqualTo(contents);
	}

	[Test]
	[AutoArguments]
	public async Task WriteAllTextAsync_ReadOnlyMemory_ShouldCreateFileWithText(
		string path, string contents)
	{
		await FileSystem.File.WriteAllTextAsync(path, contents.AsMemory(),
			CancellationToken);

		string result =
			await FileSystem.File.ReadAllTextAsync(path, CancellationToken);

		await That(result).IsEqualTo(contents);
	}

	[Test]
	[AutoArguments]
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
				CancellationToken);

			string result =
				await FileSystem.File.ReadAllTextAsync(path, CancellationToken);

			await That(result).IsEqualTo(contents)
				.Because($"{contents} should be encoded and decoded identical.");
		}
	}

	[Test]
	[AutoArguments]
	public async Task
		WriteAllTextAsync_ReadOnlyMemory_WhenDirectoryWithSameNameExists_ShouldThrowUnauthorizedAccessException(
			string path, string contents)
	{
		FileSystem.Directory.CreateDirectory(path);

		async Task Act()
		{
			await FileSystem.File.WriteAllTextAsync(path, contents.AsMemory(),
				CancellationToken);
		}

		await That(Act).Throws<UnauthorizedAccessException>().WithHResult(-2147024891);
		await That(FileSystem.Directory.Exists(path)).IsTrue();
		await That(FileSystem.File.Exists(path)).IsFalse();
	}

	[Test]
	[AutoArguments]
	public async Task
		WriteAllTextAsync_ReadOnlyMemory_WhenFileIsHidden_ShouldThrowUnauthorizedAccessException_OnWindows(
			string path, string contents)
	{
		Skip.IfNot(Test.RunsOnWindows);

		await FileSystem.File.WriteAllTextAsync(path, "", CancellationToken);
		FileSystem.File.SetAttributes(path, FileAttributes.Hidden);

		async Task Act()
		{
			await FileSystem.File.WriteAllTextAsync(path, contents.AsMemory(),
				CancellationToken);
		}

		await That(Act).Throws<UnauthorizedAccessException>().WithHResult(-2147024891);
	}

	[Test]
	[AutoArguments]
	public async Task WriteAllTextAsync_ReadOnlyMemory_WithoutEncoding_ShouldUseUtf8(
		string path)
	{
		string contents = "breuß";

		await FileSystem.File.WriteAllTextAsync(path, contents.AsMemory(), CancellationToken.None);

		byte[] bytes = FileSystem.File.ReadAllBytes(path);
		await That(bytes.Length).IsEqualTo(6);
	}
#endif
}
#endif
