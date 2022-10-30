#if FEATURE_FILESYSTEM_ASYNC
using AutoFixture;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests.FileSystem.File;

// ReSharper disable MethodHasAsyncOverload
// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class AppendAllLinesAsyncTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[AutoData]
	public async Task AppendAllLinesAsync_Cancelled_ShouldThrowTaskCanceledException(
		string path, List<string> contents)
	{
		CancellationTokenSource cts = new();
		cts.Cancel();

		Exception? exception = await Record.ExceptionAsync(() =>
			FileSystem.File.AppendAllLinesAsync(path, contents, cts.Token));

		exception.Should().BeOfType<TaskCanceledException>()
		   .Which.Message.Should().Be("A task was canceled.");
	}

	[SkippableTheory]
	[AutoData]
	public async Task
		AppendAllLinesAsync_Cancelled_WithEncoding_ShouldThrowTaskCanceledException(
			string path, List<string> contents)
	{
		CancellationTokenSource cts = new();
		cts.Cancel();

		Exception? exception = await Record.ExceptionAsync(() =>
			FileSystem.File.AppendAllLinesAsync(path, contents, Encoding.UTF8,
				cts.Token));

		exception.Should().BeOfType<TaskCanceledException>()
		   .Which.Message.Should().Be("A task was canceled.");
	}

	[SkippableTheory]
	[AutoData]
	public async Task AppendAllLinesAsync_ExistingFile_ShouldAppendLinesToFile(
		string path, List<string> previousContents, List<string> contents)
	{
		await FileSystem.File.AppendAllLinesAsync(path, previousContents);

		await FileSystem.File.AppendAllLinesAsync(path, contents);

		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.ReadAllLines(path).Should()
		   .BeEquivalentTo(previousContents.Concat(contents),
				o => o.WithStrictOrdering());
	}

	[SkippableTheory]
	[AutoData]
	public async Task AppendAllLinesAsync_MissingFile_ShouldCreateFile(
		string path, List<string> contents)
	{
		await FileSystem.File.AppendAllLinesAsync(path, contents);

		FileSystem.File.Exists(path).Should().BeTrue();
		FileSystem.File.ReadAllLines(path).Should().BeEquivalentTo(contents);
	}

	[SkippableTheory]
	[AutoData]
	public async Task AppendAllLinesAsync_ShouldEndWithNewline(string path)
	{
		string[] contents = { "foo", "bar" };
		string expectedResult = "foo" + Environment.NewLine + "bar" + Environment.NewLine;

		await FileSystem.File.AppendAllLinesAsync(path, contents);

		FileSystem.File.ReadAllText(path).Should().BeEquivalentTo(expectedResult);
	}

	[SkippableTheory]
	[ClassData(typeof(TestDataGetEncodingDifference))]
	public async Task
		AppendAllLinesAsync_WithDifferentEncoding_ShouldNotReturnWrittenText(
			string specialLine, Encoding writeEncoding, Encoding readEncoding)
	{
		string path = new Fixture().Create<string>();
		string[] lines = new Fixture().Create<string[]>();
		lines[1] = specialLine;
		await FileSystem.File.AppendAllLinesAsync(path, lines, writeEncoding);

		string[] result = FileSystem.File.ReadAllLines(path, readEncoding);

		result.Should().NotBeEquivalentTo(lines,
			$"{lines} should be different when encoding from {writeEncoding} to {readEncoding}.");
		result[0].Should().Be(lines[0]);
	}
}
#endif