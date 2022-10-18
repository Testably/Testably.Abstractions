using AutoFixture.Xunit2;
using FluentAssertions;
using System.IO;
using System.Runtime.InteropServices;
using Testably.Abstractions.Testing;
using Xunit;

namespace FileSystemConfiguration.Tests;

public class InitializationTests
{
	/// <summary>
	///     Initialize the file system in the specified <paramref name="currentDirectory" /> with<br />
	///     - a randomly named directory
	/// </summary>
	[Theory]
	[AutoData]
	public void InitializeFileSystemInSpecifiedCurrentDirectory(string currentDirectory)
	{
		FileSystemMock fileSystem = new();
		string expectedDirectory = fileSystem.Path.GetFullPath(currentDirectory);

		fileSystem.InitializeIn(currentDirectory)
			.WithASubdirectory();

		fileSystem.Directory.GetCurrentDirectory().Should()
			.Be(expectedDirectory);
	}

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
	///     The file system is automatically initialized with the main drive.<br />
	///     UNC servers (or additional drives under windows) can be added if required.
	/// </summary>
	[Fact]
	public void InitializeFileSystemWithUncDrive()
	{
		FileSystemMock fileSystem = new();
		fileSystem.DriveInfo.GetDrives().Should().HaveCount(1);

		fileSystem.WithUncDrive(@"\\unc-server");

		fileSystem.DriveInfo.GetDrives().Should().HaveCount(2);

		if (RuntimeEnvironment.Is)
	}
}