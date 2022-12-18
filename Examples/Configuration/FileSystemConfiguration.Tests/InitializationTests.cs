using AutoFixture.Xunit2;
using FluentAssertions;
using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.InteropServices;
using Testably.Abstractions.RandomSystem;
using Testably.Abstractions.Testing;
using Xunit;

namespace Testably.Abstractions.Examples.Configuration.FileSystemConfiguration.Tests;

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
		MockFileSystem fileSystem = new();
		string expectedDirectory = fileSystem.Path.GetFullPath(currentDirectory);

		fileSystem.InitializeIn(currentDirectory)
			.WithASubdirectory();

		fileSystem.Directory.GetCurrentDirectory()
			.Should()
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
		MockFileSystem fileSystem = new();
		fileSystem.Initialize()
			.WithASubdirectory()
			.WithSubdirectory("foo")
			.Initialized(s => s
				.WithAFile())
			.WithFile("bar.txt");

		fileSystem.File.Exists("bar.txt").Should().BeTrue();
		fileSystem.Directory.Exists("foo").Should().BeTrue();
		fileSystem.Directory.GetDirectories(".").Length.Should().Be(2);
	}

	/// <summary>
	///     The file system is automatically initialized with the main drive
	///     (`C:` on Windows, `/` on Linux or Mac).<br />
	///     UNC servers (or additional drives under windows) can be added if required.
	/// </summary>
	[Fact]
	public void InitializeFileSystemWithUncDrive()
	{
		MockFileSystem fileSystem = new();
		fileSystem.DriveInfo.GetDrives().Should().HaveCount(1);

		fileSystem.WithUncDrive(@"//unc-server");

		fileSystem.DriveInfo.GetDrives().Should().HaveCount(1);
		IDriveInfo drive = fileSystem.DriveInfo.New(@"//unc-server");
		drive.IsReady.Should().BeTrue();

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			fileSystem.WithDrive(@"D:");

			fileSystem.DriveInfo.GetDrives().Should().HaveCount(2);
		}
	}

	/// <summary>
	///     The drives can be configured on the <see cref="MockFileSystem" />.
	///     Per default all drives are initializes with 1GB of free space. All
	///     file operations are counted against this limit and throw an
	///     <see cref="IOException" />, when the limit is breached.
	/// </summary>
	[Fact]
	public void LimitAvailableSpaceOnDrives()
	{
		MockFileSystem fileSystem = new();
		IRandom random = fileSystem.RandomSystem.Random.Shared;
		byte[] firstFileContent = new byte[199];
		byte[] secondFileContent = new byte[2];
		random.NextBytes(firstFileContent);
		random.NextBytes(secondFileContent);

		// Limit the main drive to 200 bytes
		fileSystem.WithDrive(drive => drive.SetTotalSize(200));
		IDriveInfo mainDrive = fileSystem.DriveInfo.GetDrives().Single();
		mainDrive.AvailableFreeSpace.Should().Be(200);

		fileSystem.File.WriteAllBytes("foo", firstFileContent);
		mainDrive.AvailableFreeSpace.Should().Be(1);

		Exception? exception = Record.Exception(() =>
		{
			fileSystem.File.WriteAllBytes("bar", secondFileContent);
		});

		exception.Should().BeOfType<IOException>();
	}
}
