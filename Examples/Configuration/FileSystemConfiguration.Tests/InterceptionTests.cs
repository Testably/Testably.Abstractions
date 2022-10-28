using AutoFixture.Xunit2;
using FluentAssertions;
using System;
using Testably.Abstractions.Testing;
using Xunit;

namespace FileSystemConfiguration.Tests;

public class InterceptionTests
{
	/// <summary>
	///     Intercepting allows callbacks to be invoked before the change in the file system is performed.
	/// </summary>
	[Theory]
	[AutoData]
	public void Intercept(Exception customException)
	{
		MockFileSystem fileSystem = new();
		fileSystem.Intercept.Creating(FileSystemTypes.File,
			_ => throw customException);

		fileSystem.Directory.CreateDirectory("foo");

		Exception? exception = Record.Exception(() =>
		{
			fileSystem.File.Create("foo/bar.txt");
		});

		exception.Should().Be(customException);
		fileSystem.File.Exists("foo/bar.txt").Should().BeFalse();
	}
}