﻿using System.Collections.Generic;
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
	public void Dispose_PermanentFailure_ShouldNotThrowException(
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
			m.Contains($"'{parentOfCurrentDirectory}'"));
		receivedLogs.Should().NotContain("Cleanup was successful :-)");
	}

	[Fact]
	public void Dispose_ShouldForceDeleteCurrentDirectory()
	{
		MockFileSystem sut = new();
		List<string> receivedLogs = [];
		IDirectoryCleaner directoryCleaner =
			sut.SetCurrentDirectoryToEmptyTemporaryDirectory(logger: m => receivedLogs.Add(m));
		string currentDirectory = sut.Directory.GetCurrentDirectory();

		directoryCleaner.Dispose();
		sut.Directory.Exists(currentDirectory).Should().BeFalse();
		receivedLogs.Should().Contain("Cleanup was successful :-)");
	}

	[Fact]
	public void Dispose_ShouldResetCurrentDirectory()
	{
		MockFileSystem sut = new();
		IDirectoryCleaner directoryCleaner =
			sut.SetCurrentDirectoryToEmptyTemporaryDirectory();
		string currentDirectory = sut.Directory.GetCurrentDirectory();
		directoryCleaner.Dispose();
		sut.Directory.GetCurrentDirectory().Should().NotBe(currentDirectory);
	}

	[Theory]
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
		sut.Directory.Exists(currentDirectory).Should().BeFalse();
	}

	[Fact]
	public void InitializeBasePath_ShouldCreateDirectoryAndLogBasePath()
	{
		MockFileSystem sut = new();
		List<string> receivedLogs = [];

		using IDirectoryCleaner directoryCleaner =
			sut.SetCurrentDirectoryToEmptyTemporaryDirectory(logger: t => receivedLogs.Add(t));

		sut.Statistics.TotalCount.Should().Be(0);
		string currentDirectory = sut.Directory.GetCurrentDirectory();
		sut.Directory.Exists(currentDirectory).Should().BeTrue();
		receivedLogs.Should().Contain(m => m.Contains($"'{currentDirectory}'"));
	}
}
