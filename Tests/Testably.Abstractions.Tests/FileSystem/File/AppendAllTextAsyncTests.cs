#if FEATURE_FILESYSTEM_ASYNC
using AutoFixture;
using System.IO;
using System.Text;
using System.Threading;

namespace Testably.Abstractions.Tests.FileSystem.File;

// ReSharper disable MethodHasAsyncOverload
[FileSystemTests]
public partial class AppendAllTextAsyncTests
{
	[Theory]
	[AutoData]
	public async Task AppendAllTextAsync_Cancelled_ShouldThrowTaskCanceledException(
		string path, string contents)
	{
		using CancellationTokenSource cts = new();
		cts.Cancel();

		async Task Act() =>
			await FileSystem.File.AppendAllTextAsync(path, contents, cts.Token);

		await That(Act).Throws<TaskCanceledException>().WithHResult(-2146233029);
	}

	[Theory]
	[AutoData]
	public async Task
		AppendAllTextAsync_Cancelled_WithEncoding_ShouldThrowTaskCanceledException(
			string path, string contents)
	{
		using CancellationTokenSource cts = new();
		cts.Cancel();

		async Task Act() =>
			await FileSystem.File.AppendAllTextAsync(path, contents, Encoding.UTF8, cts.Token);

		await That(Act).Throws<TaskCanceledException>().WithHResult(-2146233029);
	}

	[Theory]
	[AutoData]
	public async Task AppendAllTextAsync_ExistingFile_ShouldAppendLinesToFile(
		string path, string previousContents, string contents)
	{
		await FileSystem.File.AppendAllTextAsync(path, previousContents,
			TestContext.Current.CancellationToken);

		await FileSystem.File.AppendAllTextAsync(path, contents,
			TestContext.Current.CancellationToken);

		await That(FileSystem.File.Exists(path)).IsTrue();
		await That(FileSystem.File.ReadAllText(path)).IsEqualTo(previousContents + contents);
	}

	[Theory]
	[AutoData]
	public async Task AppendAllTextAsync_MissingDirectory_ShouldThrowDirectoryNotFoundException(
		string missingPath, string fileName, string contents)
	{
		string filePath = FileSystem.Path.Combine(missingPath, fileName);

		async Task Act()
		{
			await FileSystem.File.AppendAllTextAsync(filePath, contents,
				TestContext.Current.CancellationToken);
		}

		await That(Act).Throws<DirectoryNotFoundException>().WithHResult(-2147024893);
	}

	[Theory]
	[AutoData]
	public async Task AppendAllTextAsync_MissingFile_ShouldCreateFile(
		string path, string contents)
	{
		await FileSystem.File.AppendAllTextAsync(path, contents,
			TestContext.Current.CancellationToken);

		await That(FileSystem.File.Exists(path)).IsTrue();
		await That(FileSystem.File.ReadAllText(path)).IsEqualTo(contents);
	}

	[Theory]
	[AutoData]
	public async Task AppendAllTextAsync_ShouldNotEndWithNewline(string path)
	{
		string contents = "foo";

		await FileSystem.File.AppendAllTextAsync(path, contents,
			TestContext.Current.CancellationToken);

		await That(FileSystem.File.Exists(path)).IsTrue();
		await That(FileSystem.File.ReadAllText(path)).IsEqualTo(contents);
	}

	[Theory]
	[AutoData]
	public async Task
		AppendAllTextAsync_WhenDirectoryWithSameNameExists_ShouldThrowUnauthorizedAccessException(
			string path, string contents)
	{
		FileSystem.Directory.CreateDirectory(path);

		async Task Act()
		{
			await FileSystem.File.AppendAllTextAsync(path, contents,
				TestContext.Current.CancellationToken);
		}

		await That(Act).Throws<UnauthorizedAccessException>().WithHResult(-2147024891);
		await That(FileSystem.Directory.Exists(path)).IsTrue();
		await That(FileSystem.File.Exists(path)).IsFalse();
	}

	[Theory]
	[ClassData(typeof(TestDataGetEncodingDifference))]
	public async Task AppendAllTextAsync_WithDifferentEncoding_ShouldNotReturnWrittenText(
		string contents, Encoding writeEncoding, Encoding readEncoding)
	{
		string path = new Fixture().Create<string>();
		await FileSystem.File.AppendAllTextAsync(path, contents, writeEncoding,
			TestContext.Current.CancellationToken);

		string[] result = FileSystem.File.ReadAllLines(path, readEncoding);

		await That(result).IsNotEqualTo([contents]);
	}

	[Theory]
	[AutoData]
	public async Task AppendAllTextAsync_WithoutEncoding_ShouldUseUtf8(
		string path)
	{
		string contents = "breuß";

		await FileSystem.File.AppendAllTextAsync(path, contents, CancellationToken.None);

		byte[] bytes = FileSystem.File.ReadAllBytes(path);
		await That(bytes.Length).IsEqualTo(6);
	}

#if FEATURE_FILE_SPAN
	[Theory]
	[AutoData]
	public async Task AppendAllTextAsync_ReadOnlyMemory_Cancelled_ShouldThrowTaskCanceledException(
		string path, string contents)
	{
		using CancellationTokenSource cts = new();
		cts.Cancel();

		async Task Act() =>
			await FileSystem.File.AppendAllTextAsync(path, contents.AsMemory(), cts.Token);

		await That(Act).Throws<TaskCanceledException>().WithHResult(-2146233029);
	}

	[Theory]
	[AutoData]
	public async Task
		AppendAllTextAsync_ReadOnlyMemory_Cancelled_WithEncoding_ShouldThrowTaskCanceledException(
			string path, string contents)
	{
		using CancellationTokenSource cts = new();
		cts.Cancel();

		async Task Act() =>
			await FileSystem.File.AppendAllTextAsync(path, contents.AsMemory(), Encoding.UTF8,
				cts.Token);

		await That(Act).Throws<TaskCanceledException>().WithHResult(-2146233029);
	}

	[Theory]
	[AutoData]
	public async Task AppendAllTextAsync_ReadOnlyMemory_ExistingFile_ShouldAppendLinesToFile(
		string path, string previousContents, string contents)
	{
		await FileSystem.File.AppendAllTextAsync(path, previousContents,
			TestContext.Current.CancellationToken);

		await FileSystem.File.AppendAllTextAsync(path, contents.AsMemory(),
			TestContext.Current.CancellationToken);

		await That(FileSystem.File.Exists(path)).IsTrue();
		await That(FileSystem.File.ReadAllText(path)).IsEqualTo(previousContents + contents);
	}

	[Theory]
	[AutoData]
	public async Task
		AppendAllTextAsync_ReadOnlyMemory_MissingDirectory_ShouldThrowDirectoryNotFoundException(
			string missingPath, string fileName, string contents)
	{
		string filePath = FileSystem.Path.Combine(missingPath, fileName);

		async Task Act()
		{
			await FileSystem.File.AppendAllTextAsync(filePath, contents.AsMemory(),
				TestContext.Current.CancellationToken);
		}

		await That(Act).Throws<DirectoryNotFoundException>().WithHResult(-2147024893);
	}

	[Theory]
	[AutoData]
	public async Task AppendAllTextAsync_ReadOnlyMemory_MissingFile_ShouldCreateFile(
		string path, string contents)
	{
		await FileSystem.File.AppendAllTextAsync(path, contents.AsMemory(),
			TestContext.Current.CancellationToken);

		await That(FileSystem.File.Exists(path)).IsTrue();
		await That(FileSystem.File.ReadAllText(path)).IsEqualTo(contents);
	}

	[Theory]
	[AutoData]
	public async Task AppendAllTextAsync_ReadOnlyMemory_ShouldNotEndWithNewline(string path)
	{
		string contents = "foo";

		await FileSystem.File.AppendAllTextAsync(path, contents.AsMemory(),
			TestContext.Current.CancellationToken);

		await That(FileSystem.File.Exists(path)).IsTrue();
		await That(FileSystem.File.ReadAllText(path)).IsEqualTo(contents);
	}

	[Theory]
	[AutoData]
	public async Task
		AppendAllTextAsync_ReadOnlyMemory_WhenDirectoryWithSameNameExists_ShouldThrowUnauthorizedAccessException(
			string path, string contents)
	{
		FileSystem.Directory.CreateDirectory(path);

		async Task Act()
		{
			await FileSystem.File.AppendAllTextAsync(path, contents.AsMemory(),
				TestContext.Current.CancellationToken);
		}

		await That(Act).Throws<UnauthorizedAccessException>().WithHResult(-2147024891);
		await That(FileSystem.Directory.Exists(path)).IsTrue();
		await That(FileSystem.File.Exists(path)).IsFalse();
	}

	[Theory]
	[ClassData(typeof(TestDataGetEncodingDifference))]
	public async Task
		AppendAllTextAsync_ReadOnlyMemory_WithDifferentEncoding_ShouldNotReturnWrittenText(
			string contents, Encoding writeEncoding, Encoding readEncoding)
	{
		string path = new Fixture().Create<string>();
		await FileSystem.File.AppendAllTextAsync(path, contents.AsMemory(), writeEncoding,
			TestContext.Current.CancellationToken);

		string[] result = FileSystem.File.ReadAllLines(path, readEncoding);

		await That(result).IsNotEqualTo([contents]);
	}

	[Theory]
	[AutoData]
	public async Task AppendAllTextAsync_ReadOnlyMemory_WithoutEncoding_ShouldUseUtf8(
		string path)
	{
		string contents = "breuß";

		await FileSystem.File.AppendAllTextAsync(path, contents.AsMemory(), CancellationToken.None);

		byte[] bytes = FileSystem.File.ReadAllBytes(path);
		await That(bytes.Length).IsEqualTo(6);
	}
#endif
}
#endif
