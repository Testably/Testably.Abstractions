using FluentAssertions;
using System;
using System.IO;
using Testably.Abstractions.Testing;
using Xunit;

namespace Testably.Abstractions.Examples.DriveManagement.Tests;

public class DriveManagementTests
{
	[Fact]
	public void DefineAdditionalDrive()
	{
		var fileSystem = new MockFileSystem()
			.WithDrive("T:\\");

		fileSystem.File.WriteAllText("T:\\foo.txt", "bar");

		fileSystem.DriveInfo.GetDrives()
			.Should().Contain(d => d.Name == "T:\\");
		fileSystem.File.Exists("T:\\foo.txt").Should().BeTrue();
	}

	[Fact]
	public void ChangeDefaultDrive()
	{
		var fileSystem = new MockFileSystem()
			.InitializeIn("U:\\sub\\directory")
			.FileSystem;

		fileSystem.File.WriteAllText("foo.txt", "bar");

		fileSystem.DriveInfo.GetDrives()
			.Should().Contain(d => d.Name == "U:\\");
		fileSystem.File.Exists("U:\\sub\\directory\\foo.txt")
			.Should().BeTrue();
	}

	[Fact]
	public void LimitAvailableDriveSize()
	{
		var fileSystem = new MockFileSystem()
			.WithDrive("C:\\", d => d.SetTotalSize(100));
		var drive = fileSystem.GetDefaultDrive();
		var largeFileContent = new byte[90];
		Random.Shared.NextBytes(largeFileContent);

		drive.AvailableFreeSpace.Should().Be(100);
		fileSystem.File.WriteAllText("foo.txt", "This is a text with 29 bytes.");
		drive.AvailableFreeSpace.Should().Be(71);
		fileSystem.File.AppendAllText("foo.txt", "Another 17 bytes.");
		drive.AvailableFreeSpace.Should().Be(54);

		var exception = Record.Exception(() =>
		{
			fileSystem.File.WriteAllBytes("bar.bin", largeFileContent);
		});
		exception.Should().BeOfType<IOException>();

		drive.AvailableFreeSpace.Should().Be(54);
		fileSystem.File.Delete("foo.txt");
		drive.AvailableFreeSpace.Should().Be(100);
		fileSystem.File.WriteAllBytes("bar.bin", largeFileContent);
		drive.AvailableFreeSpace.Should().Be(10);
	}
}

