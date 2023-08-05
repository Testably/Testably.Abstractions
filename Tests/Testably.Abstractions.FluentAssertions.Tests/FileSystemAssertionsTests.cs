using AutoFixture.Xunit2;
using FluentAssertions;
using System;
using Testably.Abstractions.Testing;
using Xunit;

namespace Testably.Abstractions.FluentAssertions.Tests;

public class FileSystemAssertionsTests
{
	[Theory]
	[AutoData]
	public void HaveDirectory_WithDirectory_ShouldNotThrow(string path)
	{
		MockFileSystem fileSystem = new();
		fileSystem.Initialize()
			.WithSubdirectory(path);

		fileSystem.Should().HaveDirectory(path);
	}

	[Theory]
	[AutoData]
	public void HaveDirectory_WithMultipleDirectories_ShouldNotThrow(string path1, string path2)
	{
		MockFileSystem fileSystem = new();
		fileSystem.Initialize()
			.WithSubdirectory(path1)
			.WithSubdirectory(path2);

		fileSystem.Should().HaveDirectory(path1).And.HaveDirectory(path2);
	}

	[Theory]
	[AutoData]
	public void HaveDirectory_WithoutDirectory_ShouldThrow(
		string path,
		string because)
	{
		MockFileSystem fileSystem = new();

		Exception? exception = Record.Exception(() =>
		{
			fileSystem.Should().HaveDirectory(path, because);
		});

		exception.Should().NotBeNull();
		exception!.Message.Should().Contain(path);
		exception.Message.Should().Contain(because);
	}

	[Theory]
	[AutoData]
	public void HaveFile_WithFile_ShouldNotThrow(string path)
	{
		MockFileSystem fileSystem = new();
		fileSystem.Initialize()
			.WithFile(path);

		fileSystem.Should().HaveFile(path);
	}

	[Theory]
	[AutoData]
	public void HaveFile_WithMultipleDirectories_ShouldNotThrow(string path1, string path2)
	{
		MockFileSystem fileSystem = new();
		fileSystem.Initialize()
			.WithFile(path1)
			.WithFile(path2);

		fileSystem.Should().HaveFile(path1).And.HaveFile(path2);
	}

	[Theory]
	[AutoData]
	public void HaveFile_WithoutFile_ShouldThrow(string path, string because)
	{
		MockFileSystem fileSystem = new();

		Exception? exception = Record.Exception(() =>
		{
			fileSystem.Should().HaveFile(path, because);
		});

		exception.Should().NotBeNull();
		exception!.Message.Should().Contain(path);
		exception.Message.Should().Contain(because);
	}
}
