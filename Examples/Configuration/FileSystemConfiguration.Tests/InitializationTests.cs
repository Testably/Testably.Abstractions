using AutoFixture.Xunit2;
using FluentAssertions;
using Testably.Abstractions.Testing;
using Xunit;

namespace FileSystemConfiguration.Tests;

public class InitializationTests
{
	/// <summary>
	///     Initialize the file system in the root directory with<br />
	///     - a randomly named directory
	///     - a directory named "foo" which contains a randomly named file
	///     - a file named "bar.txt"
	/// </summary>
	[Fact]
	public void InitializeFileSystemInTheRootDirectory()
	{
		FileSystemMock fileSystem = new();
		fileSystem.Initialize()
			.WithASubdirectory()
			.WithSubdirectory("foo").Initialized(s => s
				.WithAFile())
			.WithFile("bar.txt");

		fileSystem.File.Exists("bar.txt").Should().BeTrue();
		fileSystem.Directory.Exists("foo").Should().BeTrue();
		fileSystem.Directory.GetDirectories(".").Length.Should().Be(2);
	}

	/// <summary>
	///     Initialize the file system in the specified <paramref name="currentDirectory" /> with<br />
	///     - a randomly named directory
	/// </summary>
	[Theory]
	[AutoData]
	public void InitializeFileSystemInAGivenCurrentDirectory(string currentDirectory)
	{
		FileSystemMock fileSystem = new();
		var expectedDirectory = fileSystem.Path.GetFullPath(currentDirectory);

		fileSystem.InitializeIn(currentDirectory)
			.WithASubdirectory();

		fileSystem.Directory.GetCurrentDirectory().Should()
			.Be(expectedDirectory);
	}
}