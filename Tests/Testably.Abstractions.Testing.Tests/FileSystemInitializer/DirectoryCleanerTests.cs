using System.Collections.Generic;
using System.IO;
using System.Linq;
using Testably.Abstractions.Testing.FileSystemInitializer;
using Testably.Abstractions.Testing.Tests.TestHelpers;

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

	[SkippableTheory]
	[AutoData]
	public void Dispose_PermanentFailure_ShouldNotThrowException(
		Exception exception)
	{
		MockFileSystem sut = new();
		List<string> receivedLogs = new();
		IDirectoryCleaner directoryCleaner =
			sut.SetCurrentDirectoryToEmptyTemporaryDirectory(logger: m => receivedLogs.Add(m));
		string currentDirectory = sut.Directory.GetCurrentDirectory();
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

		receivedException.Should().BeNull();
		exceptionCount.Should().BeGreaterThan(10);
		foreach (string retryMessage in Enumerable
			.Range(1, 10)
			.Select(i => $"Retry again {i} times"))
		{
			receivedLogs.Should().Contain(m => m.Contains(retryMessage));
		}

		receivedLogs.Should().Contain(m =>
			m.Contains(exception.Message) &&
			m.Contains($"'{currentDirectory}'"));
		receivedLogs.Should().NotContain("Cleanup was successful :-)");
	}

	[SkippableFact]
	public void Dispose_ShouldForceDeleteCurrentDirectory()
	{
		MockFileSystem sut = new();
		List<string> receivedLogs = new();
		IDirectoryCleaner directoryCleaner =
			sut.SetCurrentDirectoryToEmptyTemporaryDirectory(logger: m => receivedLogs.Add(m));
		string currentDirectory = sut.Directory.GetCurrentDirectory();

		directoryCleaner.Dispose();
		sut.Should().NotHaveDirectory(currentDirectory);
		receivedLogs.Should().Contain("Cleanup was successful :-)");
	}

	[SkippableFact]
	public void Dispose_ShouldResetCurrentDirectory()
	{
		MockFileSystem sut = new();
		IDirectoryCleaner directoryCleaner =
			sut.SetCurrentDirectoryToEmptyTemporaryDirectory();
		string currentDirectory = sut.Directory.GetCurrentDirectory();
		directoryCleaner.Dispose();
		sut.Directory.GetCurrentDirectory().Should().NotBe(currentDirectory);
	}

	[SkippableTheory]
	[AutoData]
	public void Dispose_TemporaryFailure_ShouldRetryAgain(
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
		sut.Should().NotHaveDirectory(currentDirectory);
	}

	[SkippableFact]
	public void InitializeBasePath_ShouldCreateDirectoryAndLogBasePath()
	{
		MockFileSystem sut = new();
		List<string> receivedLogs = new();

		using IDirectoryCleaner directoryCleaner =
			sut.SetCurrentDirectoryToEmptyTemporaryDirectory(logger: t => receivedLogs.Add(t));

		sut.StatisticsRegistration.TotalCount.Should().Be(0);
		string currentDirectory = sut.Directory.GetCurrentDirectory();
		sut.Should().HaveDirectory(currentDirectory);
		receivedLogs.Should().Contain(m => m.Contains($"'{currentDirectory}'"));
	}
}
