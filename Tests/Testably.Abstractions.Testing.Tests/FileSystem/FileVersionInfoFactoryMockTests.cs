﻿using System.Diagnostics;

namespace Testably.Abstractions.Testing.Tests.FileSystem;

public sealed class FileVersionInfoFactoryMockTests
{
	[Theory]
	[AutoData]
	public void WhenRegistered_ShouldReturnFileVersionInfoWithRegisteredValues(
		string comments)
	{
		MockFileSystem fileSystem = new();
		fileSystem.Initialize().WithFile("abc.foo");
		fileSystem.WithFileVersionInfo("*.foo", b => b.SetComments(comments));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("abc.foo");

		result.Comments.Should().Be(comments);
	}

	[Theory]
	[AutoData]
	public void WhenRegisteredUnderDifferentName_ShouldReturnDefaultValues(
		string comments)
	{
		MockFileSystem fileSystem = new();
		fileSystem.Initialize().WithFile("abc.bar");
		fileSystem.WithFileVersionInfo("*.foo", b => b.SetComments(comments));

		IFileVersionInfo result = fileSystem.FileVersionInfo.GetVersionInfo("abc.bar");

		result.Comments.Should().BeNull();
	}
}