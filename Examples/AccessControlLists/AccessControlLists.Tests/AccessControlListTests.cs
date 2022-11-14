using AutoFixture.Xunit2;
using FluentAssertions;
using System;
using System.IO;
using Testably.Abstractions.Testing;
using Xunit;

namespace AccessControlLists.Tests;

public class AccessControlListTests
{
	public AccessControlListTests()
	{
		FileSystem = new MockFileSystem();
	}

	public MockFileSystem FileSystem { get; }

	[Theory]
	[AutoData]
	public void ReadAllText_DeniedPath_ShouldThrowIOException(string grantedPath, string deniedPath)
	{
		FileSystem.File.WriteAllText(grantedPath, "foo");
		FileSystem.File.WriteAllText(deniedPath, "bar");
		FileSystem.WithAccessControlStrategy(new CustomAccessControlStrategy(
			path => path.Contains(grantedPath)));

		string result = FileSystem.File.ReadAllText(grantedPath);
		result.Should().Be("foo");

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.ReadAllText(deniedPath);
		});
		exception.Should().BeOfType<IOException>()
			.Which.Message.Should()
			.Be($"Access to the path '{FileSystem.Path.GetFullPath(deniedPath)}' is denied.");
	}
}
