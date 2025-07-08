using System.Collections.Generic;
using System.IO;
using System.Linq;
using Testably.Abstractions.Testing.Initializer;

namespace Testably.Abstractions.Testing.Tests.FileSystemInitializer;

[Collection(nameof(IDirectoryCleaner))]
public class DirectoryCleanerTests
{
	#region Test Setup

	public DirectoryCleanerTests()
	{
		Skip.If(Test.RunsOnMac, "No access to temporary directories under `/private`");
	}

	#endregion

	[Theory]
	[AutoData]
	public async Task Dispose_PermanentFailure_ShouldNotThrowException(
		Exception exception)
	{
		MockFileSystem sut = new();
		List<string> receivedLogs = [];
		IDirectoryCleaner directoryCleaner =
			sut.SetCurrentDirectoryToEmptyTemporaryDirectory(logger: m => receivedLogs.Add(m));
		string currentDirectory = sut.Directory.GetCurrentDirectory();
		string parentOfCurrentDirectory = sut.Path.GetDirectoryName(currentDirectory)!;
		int exceptionCount = 0;
		sut.Intercept.Event(_ =>
			{
				exceptionCount++;
				throw exception;
			},
			c => c.ChangeType.HasFlag(WatcherChangeTypes.Deleted));

		Exception? receivedException = Record.Exception(() =>
		{
			directoryCleaner.Dispose();
		});

		await That(receivedException).IsNull();
		await That(exceptionCount).IsGreaterThan(10);
		foreach (string retryMessage in Enumerable
			.Range(1, 10)
			.Select(i => $"Retry again {i} times"))
		{
			await That(receivedLogs).Contains(m => m.Contains(retryMessage));
		}

		await That(receivedLogs).Contains(m =>
			m.Contains(exception.Message) &&
			m.Contains($"'{parentOfCurrentDirectory}'"));
		await That(receivedLogs).DoesNotContain("Cleanup was successful :-)");
	}

	[Fact]
	public async Task Dispose_ShouldForceDeleteCurrentDirectory()
	{
		MockFileSystem sut = new();
		List<string> receivedLogs = [];
		IDirectoryCleaner directoryCleaner =
			sut.SetCurrentDirectoryToEmptyTemporaryDirectory(logger: m => receivedLogs.Add(m));
		string currentDirectory = sut.Directory.GetCurrentDirectory();

		directoryCleaner.Dispose();
		await That(sut.Directory.Exists(currentDirectory)).IsFalse();
		await That(receivedLogs).Contains("Cleanup was successful :-)");
	}

	[Fact]
	public async Task Dispose_ShouldResetCurrentDirectory()
	{
		MockFileSystem sut = new();
		IDirectoryCleaner directoryCleaner =
			sut.SetCurrentDirectoryToEmptyTemporaryDirectory();
		string currentDirectory = sut.Directory.GetCurrentDirectory();
		directoryCleaner.Dispose();
		await That(sut.Directory.GetCurrentDirectory()).IsNotEqualTo(currentDirectory);
	}

	[Theory]
	[AutoData]
	public async Task Dispose_TemporaryFailure_ShouldRetryAgain(
		Exception exception)
	{
		MockFileSystem sut = new();
		IDirectoryCleaner directoryCleaner =
			sut.SetCurrentDirectoryToEmptyTemporaryDirectory();
		string currentDirectory = sut.Directory.GetCurrentDirectory();
		int exceptionCount = 0;
		sut.Intercept.Event(_ =>
			{
				exceptionCount++;
				throw exception;
			},
			c => exceptionCount < 3 &&
			     c.ChangeType.HasFlag(WatcherChangeTypes.Deleted));

		directoryCleaner.Dispose();
		await That(sut.Directory.Exists(currentDirectory)).IsFalse();
	}

	[Fact]
	public async Task InitializeBasePath_ShouldCreateDirectoryAndLogBasePath()
	{
		MockFileSystem sut = new();
		List<string> receivedLogs = [];

		using IDirectoryCleaner directoryCleaner =
			sut.SetCurrentDirectoryToEmptyTemporaryDirectory(logger: t => receivedLogs.Add(t));

		await That(sut.Statistics.TotalCount).IsEqualTo(0);
		string currentDirectory = sut.Directory.GetCurrentDirectory();
		await That(sut.Directory.Exists(currentDirectory)).IsTrue();
		await That(receivedLogs).Contains(m => m.Contains($"'{currentDirectory}'"));
	}
}
