using AutoFixture.Xunit2;
using FluentAssertions;
using System;
using Testably.Abstractions.Testing;
using Xunit;

namespace Testably.Abstractions.FluentAssertions.Tests;

public class DirectoryInfoAssertionsTests
{
	[Theory]
	[AutoData]
	public void HaveFile_WithFile_ShouldNotThrow(string directoryName,
		string fileName)
	{
		MockFileSystem fileSystem = new();
		fileSystem.Initialize()
			.WithSubdirectory(directoryName).Initialized(d => d
				.WithFile(fileName));
		DirectoryInfoAssertions? sut = fileSystem.Should().HaveDirectory(directoryName).Which;

		sut.HaveFile(fileName);
	}

	[Theory]
	[AutoData]
	public void HaveFile_WithoutFile_ShouldThrow(string directoryName,
		string fileName)
	{
		MockFileSystem fileSystem = new();
		fileSystem.Initialize()
			.WithSubdirectory(directoryName);
		DirectoryInfoAssertions? sut = fileSystem.Should().HaveDirectory(directoryName).Which;

		Exception? exception = Record.Exception(() =>
		{
			sut.HaveFile(fileName);
		});

		exception.Should().NotBeNull();
	}
}
