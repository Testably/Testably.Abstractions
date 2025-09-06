#if FEATURE_FILESYSTEM_ASYNC
using AutoFixture;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Testably.Abstractions.Tests.FileSystem.File;

// ReSharper disable MethodHasAsyncOverload
[FileSystemTests]
public partial class AppendAllLinesAsyncTests
{
	[Theory]
	[AutoData]
	public async Task AppendAllLinesAsync_Cancelled_ShouldThrowTaskCanceledException(
		string path, List<string> contents)
	{
		using CancellationTokenSource cts = new();
		cts.Cancel();

		async Task Act() =>
			await FileSystem.File.AppendAllLinesAsync(path, contents, cts.Token);

		await That(Act).Throws<TaskCanceledException>().WithHResult(-2146233029);
	}

	[Theory]
	[AutoData]
	public async Task
		AppendAllLinesAsync_Cancelled_WithEncoding_ShouldThrowTaskCanceledException(
			string path, List<string> contents)
	{
		using CancellationTokenSource cts = new();
		cts.Cancel();

		async Task Act() =>
			await FileSystem.File.AppendAllLinesAsync(path, contents, Encoding.UTF8,
				cts.Token);

		await That(Act).Throws<TaskCanceledException>().WithHResult(-2146233029);
	}

	[Theory]
	[AutoData]
	public async Task AppendAllLinesAsync_Enumerable_WithoutEncoding_ShouldUseUtf8(
		string path)
	{
		string[] contents = ["breuß"];

		await FileSystem.File.AppendAllLinesAsync(path, contents.AsEnumerable(),
			CancellationToken.None);

		byte[] bytes = FileSystem.File.ReadAllBytes(path);
		await That(bytes.Length).IsEqualTo(6 + Environment.NewLine.Length);
	}

	[Theory]
	[AutoData]
	public async Task AppendAllLinesAsync_ExistingFile_ShouldAppendLinesToFile(
		string path, List<string> previousContents, List<string> contents)
	{
		string expectedContent = string.Join(Environment.NewLine, previousContents.Concat(contents))
		                         + Environment.NewLine;
		await FileSystem.File.AppendAllLinesAsync(path, previousContents,
			TestContext.Current.CancellationToken);

		await FileSystem.File.AppendAllLinesAsync(path, contents,
			TestContext.Current.CancellationToken);

		await That(FileSystem.File.Exists(path)).IsTrue();
		await That(FileSystem.File.ReadAllText(path)).IsEqualTo(expectedContent);
	}

	[Theory]
	[AutoData]
	public async Task AppendAllLinesAsync_MissingDirectory_ShouldThrowDirectoryNotFoundException(
		string missingPath, string fileName, List<string> contents)
	{
		string filePath = FileSystem.Path.Combine(missingPath, fileName);

		async Task Act()
		{
			await FileSystem.File.AppendAllLinesAsync(filePath, contents,
				TestContext.Current.CancellationToken);
		}

		await That(Act).Throws<DirectoryNotFoundException>().WithHResult(-2147024893);
	}

	[Theory]
	[AutoData]
	public async Task AppendAllLinesAsync_MissingFile_ShouldCreateFile(
		string path, List<string> contents)
	{
		string expectedContent = string.Join(Environment.NewLine, contents)
		                         + Environment.NewLine;

		await FileSystem.File.AppendAllLinesAsync(path, contents,
			TestContext.Current.CancellationToken);

		await That(FileSystem.File.Exists(path)).IsTrue();
		await That(FileSystem.File.ReadAllText(path)).IsEqualTo(expectedContent);
	}

	[Theory]
	[AutoData]
	public async Task AppendAllLinesAsync_NullContent_ShouldThrowArgumentNullException(
		string path)
	{
		async Task Act()
		{
			await FileSystem.File.AppendAllLinesAsync(path, null!,
				TestContext.Current.CancellationToken);
		}

		await That(Act).Throws<ArgumentNullException>()
			.WithHResult(-2147467261).And
			.WithParamName("contents");
	}

	[Theory]
	[AutoData]
	public async Task AppendAllLinesAsync_NullEncoding_ShouldThrowArgumentNullException(
		string path)
	{
		async Task Act()
		{
			await FileSystem.File.AppendAllLinesAsync(path, [], null!,
				TestContext.Current.CancellationToken);
		}

		await That(Act).Throws<ArgumentNullException>()
			.WithHResult(-2147467261).And
			.WithParamName("encoding");
	}

	[Theory]
	[AutoData]
	public async Task AppendAllLinesAsync_ShouldEndWithNewline(string path)
	{
		string[] contents = ["foo", "bar"];
		string expectedResult = "foo" + Environment.NewLine + "bar" + Environment.NewLine;

		await FileSystem.File.AppendAllLinesAsync(path, contents,
			TestContext.Current.CancellationToken);

		await That(FileSystem.File.Exists(path)).IsTrue();
		await That(FileSystem.File.ReadAllText(path)).IsEqualTo(expectedResult);
	}

	[Theory]
	[AutoData]
	public async Task
		AppendAllLinesAsync_WhenDirectoryWithSameNameExists_ShouldThrowUnauthorizedAccessException(
			string path, string[] contents)
	{
		FileSystem.Directory.CreateDirectory(path);

		async Task Act()
		{
			await FileSystem.File.AppendAllLinesAsync(path, contents,
				TestContext.Current.CancellationToken);
		}

		await That(Act).Throws<UnauthorizedAccessException>().WithHResult(-2147024891);
		await That(FileSystem.Directory.Exists(path)).IsTrue();
		await That(FileSystem.File.Exists(path)).IsFalse();
	}

	[Theory]
	[ClassData(typeof(TestDataGetEncodingDifference))]
	public async Task
		AppendAllLinesAsync_WithDifferentEncoding_ShouldNotReturnWrittenText(
			string specialLine, Encoding writeEncoding, Encoding readEncoding)
	{
		string path = new Fixture().Create<string>();
		string[] lines = new Fixture().Create<string[]>();
		lines[1] = specialLine;
		await FileSystem.File.AppendAllLinesAsync(path, lines, writeEncoding,
			TestContext.Current.CancellationToken);

		string[] result = FileSystem.File.ReadAllLines(path, readEncoding);

		await That(result).IsNotEqualTo(lines).InAnyOrder();
		await That(result[0]).IsEqualTo(lines[0]);
	}
}
#endif
