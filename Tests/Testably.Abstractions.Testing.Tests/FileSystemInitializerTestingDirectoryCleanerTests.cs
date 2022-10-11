using System.Collections.Generic;
using System.Linq;

namespace Testably.Abstractions.Testing.Tests;

public class FileSystemInitializerTestingDirectoryCleanerTests
{
	[Theory]
	[AutoData]
	public void
		Dispose_PermanentError_ShouldThrowException(Exception exception)
	{
		FileSystemMock sut = new();
		List<string> receivedLogs = new();
		FileSystemInitializer.IDirectoryCleaner directoryCleaner =
			sut.SetCurrentDirectoryToEmptyTemporaryDirectory(m => receivedLogs.Add(m));
		string currentDirectory = sut.Directory.GetCurrentDirectory();
		int exceptionCount = 0;
		sut.Intercept.Change(_ =>
			{
				exceptionCount++;
				throw exception;
			},
			c => c.Type.HasFlag(FileSystemMock.ChangeTypes.Deleted));

		Exception? receivedException = Record.Exception(() =>
		{
			directoryCleaner.Dispose();
		});

		receivedException.Should().Be(exception);
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

	[Fact]
	public void
		Dispose_ShouldForceDeleteCurrentDirectory()
	{
		FileSystemMock sut = new();
		List<string> receivedLogs = new();
		FileSystemInitializer.IDirectoryCleaner directoryCleaner =
			sut.SetCurrentDirectoryToEmptyTemporaryDirectory(m => receivedLogs.Add(m));
		string currentDirectory = sut.Directory.GetCurrentDirectory();

		directoryCleaner.Dispose();
		sut.Directory.Exists(currentDirectory).Should().BeFalse();
		receivedLogs.Should().Contain("Cleanup was successful :-)");
	}

	[Theory]
	[AutoData]
	public void
		Dispose_ShouldResetCurrentDirectory()
	{
		FileSystemMock sut = new();
		FileSystemInitializer.IDirectoryCleaner directoryCleaner =
			sut.SetCurrentDirectoryToEmptyTemporaryDirectory();
		string currentDirectory = sut.Directory.GetCurrentDirectory();
		directoryCleaner.Dispose();
		sut.Directory.GetCurrentDirectory().Should().NotBe(currentDirectory);
	}

	[Theory]
	[AutoData]
	public void
		Dispose_TemporaryError_ShouldRetryAgain(Exception exception)
	{
		FileSystemMock sut = new();
		FileSystemInitializer.IDirectoryCleaner directoryCleaner =
			sut.SetCurrentDirectoryToEmptyTemporaryDirectory();
		string currentDirectory = sut.Directory.GetCurrentDirectory();
		int exceptionCount = 0;
		sut.Intercept.Change(_ =>
			{
				exceptionCount++;
				throw exception;
			},
			c => exceptionCount < 3 &&
			     c.Type.HasFlag(FileSystemMock.ChangeTypes.Deleted));

		directoryCleaner.Dispose();
		sut.Directory.Exists(currentDirectory).Should().BeFalse();
	}

	[Fact]
	public void InitializeBasePath_ShouldCreateDirectoryAndLogBasePath()
	{
		FileSystemMock sut = new();
		List<string> receivedLogs = new();

		using FileSystemInitializer.IDirectoryCleaner directoryCleaner =
			sut.SetCurrentDirectoryToEmptyTemporaryDirectory(t => receivedLogs.Add(t));

		string currentDirectory = sut.Directory.GetCurrentDirectory();
		sut.Directory.Exists(currentDirectory).Should().BeTrue();
		receivedLogs.Should().Contain(m => m.Contains($"'{currentDirectory}'"));
	}
}