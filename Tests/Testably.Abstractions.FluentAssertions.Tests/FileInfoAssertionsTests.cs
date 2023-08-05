using AutoFixture.Xunit2;
using FluentAssertions;
using System;
using Testably.Abstractions.Testing;
using Testably.Abstractions.Testing.FileSystemInitializer;
using Xunit;

namespace Testably.Abstractions.FluentAssertions.Tests;

public class FileInfoAssertionsTests
{
	[Theory]
	[AutoData]
	public void IsNotReadOnly_WithReadOnlyFile_ShouldThrow(
		FileDescription fileDescription,
		string because)
	{
		fileDescription.IsReadOnly = true;
		MockFileSystem fileSystem = new();
		fileSystem.Initialize()
			.With(fileDescription);
		FileInfoAssertions? sut = fileSystem.Should().HaveFile(fileDescription.Name).Which;

		Exception? exception = Record.Exception(() =>
		{
			sut.IsNotReadOnly(because);
		});

		exception.Should().NotBeNull();
		exception!.Message.Should().Contain(fileDescription.Name);
		exception.Message.Should().Contain(because);
	}

	[Theory]
	[AutoData]
	public void IsNotReadOnly_WithWritableFile_ShouldNotThrow(FileDescription fileDescription)
	{
		fileDescription.IsReadOnly = false;
		MockFileSystem fileSystem = new();
		fileSystem.Initialize()
			.With(fileDescription);
		FileInfoAssertions? sut = fileSystem.Should().HaveFile(fileDescription.Name).Which;

		sut.IsNotReadOnly();
	}

	[Theory]
	[AutoData]
	public void IsReadOnly_WithReadOnlyFile_ShouldNotThrow(FileDescription fileDescription)
	{
		fileDescription.IsReadOnly = true;
		MockFileSystem fileSystem = new();
		fileSystem.Initialize()
			.With(fileDescription);
		FileInfoAssertions? sut = fileSystem.Should().HaveFile(fileDescription.Name).Which;

		sut.IsReadOnly();
	}

	[Theory]
	[AutoData]
	public void IsReadOnly_WithWritableFile_ShouldThrow(
		FileDescription fileDescription,
		string because)
	{
		fileDescription.IsReadOnly = false;
		MockFileSystem fileSystem = new();
		fileSystem.Initialize()
			.With(fileDescription);
		FileInfoAssertions? sut = fileSystem.Should().HaveFile(fileDescription.Name).Which;

		Exception? exception = Record.Exception(() =>
		{
			sut.IsReadOnly(because);
		});

		exception.Should().NotBeNull();
		exception!.Message.Should().Contain(fileDescription.Name);
		exception.Message.Should().Contain(because);
	}
}
