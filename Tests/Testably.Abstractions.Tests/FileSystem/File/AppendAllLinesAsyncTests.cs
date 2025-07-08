#if FEATURE_FILESYSTEM_ASYNC
using AutoFixture;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

		Exception? exception = await Record.ExceptionAsync(() =>
			FileSystem.File.AppendAllLinesAsync(path, contents, cts.Token));

		exception.Should().BeException<TaskCanceledException>(hResult: -2146233029);
	}

	[Theory]
	[AutoData]
	public async Task
		AppendAllLinesAsync_Cancelled_WithEncoding_ShouldThrowTaskCanceledException(
			string path, List<string> contents)
	{
		using CancellationTokenSource cts = new();
		cts.Cancel();

		Exception? exception = await Record.ExceptionAsync(() =>
			FileSystem.File.AppendAllLinesAsync(path, contents, Encoding.UTF8,
				cts.Token));

		exception.Should().BeException<TaskCanceledException>(hResult: -2146233029);
	}

	[Theory]
	[AutoData]
	public async Task AppendAllLinesAsync_ExistingFile_ShouldAppendLinesToFile(
		string path, List<string> previousContents, List<string> contents)
	{
		string expectedContent = string.Join(Environment.NewLine, previousContents.Concat(contents))
		                         + Environment.NewLine;
		await FileSystem.File.AppendAllLinesAsync(path, previousContents, TestContext.Current.CancellationToken);

		await FileSystem.File.AppendAllLinesAsync(path, contents, TestContext.Current.CancellationToken);

		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.ReadAllText(path).Should().BeEquivalentTo(expectedContent);
	}

	[Theory]
	[AutoData]
	public async Task AppendAllLinesAsync_MissingDirectory_ShouldThrowDirectoryNotFoundException(
		string missingPath, string fileName, List<string> contents)
	{
		string filePath = FileSystem.Path.Combine(missingPath, fileName);

		async Task Act()
		{
			await FileSystem.File.AppendAllLinesAsync(filePath, contents, TestContext.Current.CancellationToken);
		}

		Exception? exception = await Record.ExceptionAsync(Act);

		exception.Should().BeException<DirectoryNotFoundException>(hResult: -2147024893);
	}

	[Theory]
	[AutoData]
	public async Task AppendAllLinesAsync_MissingFile_ShouldCreateFile(
		string path, List<string> contents)
	{
		string expectedContent = string.Join(Environment.NewLine, contents)
		                         + Environment.NewLine;

		await FileSystem.File.AppendAllLinesAsync(path, contents, TestContext.Current.CancellationToken);

		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.ReadAllText(path).Should().BeEquivalentTo(expectedContent);
	}

	[Theory]
	[AutoData]
	public async Task AppendAllLinesAsync_NullContent_ShouldThrowArgumentNullException(
		string path)
	{
		async Task Act()
		{
			await FileSystem.File.AppendAllLinesAsync(path, null!, TestContext.Current.CancellationToken);
		}

		Exception? exception = await Record.ExceptionAsync(Act);

		exception.Should().BeException<ArgumentNullException>(
			hResult: -2147467261,
			paramName: "contents");
	}

	[Theory]
	[AutoData]
	public async Task AppendAllLinesAsync_NullEncoding_ShouldThrowArgumentNullException(
		string path)
	{
		async Task Act()
		{
			await FileSystem.File.AppendAllLinesAsync(path, new List<string>(), null!, TestContext.Current.CancellationToken);
		}

		Exception? exception = await Record.ExceptionAsync(Act);

		exception.Should().BeException<ArgumentNullException>(
			hResult: -2147467261,
			paramName: "encoding");
	}

	[Theory]
	[AutoData]
	public async Task AppendAllLinesAsync_ShouldEndWithNewline(string path)
	{
		string[] contents = ["foo", "bar"];
		string expectedResult = "foo" + Environment.NewLine + "bar" + Environment.NewLine;

		await FileSystem.File.AppendAllLinesAsync(path, contents, TestContext.Current.CancellationToken);

		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.ReadAllText(path).Should().BeEquivalentTo(expectedResult);
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
			await FileSystem.File.AppendAllLinesAsync(path, contents, TestContext.Current.CancellationToken);
		}

		Exception? exception = await Record.ExceptionAsync(Act);

		exception.Should().BeException<UnauthorizedAccessException>(
			hResult: -2147024891);
		FileSystem.Directory.Exists(path).Should().BeTrue();
		FileSystem.File.Exists(path).Should().BeFalse();
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
		await FileSystem.File.AppendAllLinesAsync(path, lines, writeEncoding, TestContext.Current.CancellationToken);

		string[] result = FileSystem.File.ReadAllLines(path, readEncoding);

		await That(result).IsNotEqualTo(lines).InAnyOrder();
		await That(result[0]).IsEqualTo(lines[0]);
	}
}
#endif
