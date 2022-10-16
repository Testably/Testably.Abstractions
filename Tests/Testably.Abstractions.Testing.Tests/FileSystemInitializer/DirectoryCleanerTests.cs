using System.Collections.Generic;
using System.IO;
using System.Linq;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.FileSystemInitializer;

public class DirectoryCleanerTests
{
	public DirectoryCleanerTests()
	{
		Skip.If(Test.RunsOnMac, "No access to temporary directories under `/private`");
	}

	[SkippableTheory]
	[AutoData]
	public void Dispose_PermanentFailure_ShouldNotThrowException(
		Exception exception)
	{
		Testing.FileSystemMock sut = new();
		List<string> receivedLogs = new();
		Testing.FileSystemInitializer.IDirectoryCleaner directoryCleaner =
			sut.SetCurrentDirectoryToEmptyTemporaryDirectory(m => receivedLogs.Add(m));
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
		Testing.FileSystemMock sut = new();
		List<string> receivedLogs = new();
		Testing.FileSystemInitializer.IDirectoryCleaner directoryCleaner =
			sut.SetCurrentDirectoryToEmptyTemporaryDirectory(m => receivedLogs.Add(m));
		string currentDirectory = sut.Directory.GetCurrentDirectory();

		directoryCleaner.Dispose();
		sut.Directory.Exists(currentDirectory).Should().BeFalse();
		receivedLogs.Should().Contain("Cleanup was successful :-)");
	}

	[SkippableFact]
	public void Dispose_ShouldResetCurrentDirectory()
	{
		Testing.FileSystemMock sut = new();
		Testing.FileSystemInitializer.IDirectoryCleaner directoryCleaner =
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
		Testing.FileSystemMock sut = new();
		Testing.FileSystemInitializer.IDirectoryCleaner directoryCleaner =
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
		sut.Directory.Exists(currentDirectory).Should().BeFalse();
	}

	[SkippableFact]
	public void InitializeBasePath_ShouldCreateDirectoryAndLogBasePath()
	{
		Testing.FileSystemMock sut = new();
		List<string> receivedLogs = new();

		using Testing.FileSystemInitializer.IDirectoryCleaner directoryCleaner =
			sut.SetCurrentDirectoryToEmptyTemporaryDirectory(t => receivedLogs.Add(t));

		string currentDirectory = sut.Directory.GetCurrentDirectory();
		sut.Directory.Exists(currentDirectory).Should().BeTrue();
		receivedLogs.Should().Contain(m => m.Contains($"'{currentDirectory}'"));
	}
}