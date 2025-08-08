using aweXpect;
using System;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Testably.Abstractions.Testing;
using Xunit;

namespace Testably.Abstractions.Examples.DriveManagement.Tests;

public class DriveManagementTests
{
	[Fact]
	public async Task ChangeDefaultDrive()
	{
		MockFileSystem fileSystem = new MockFileSystem()
			.InitializeIn("U:\\sub\\directory")
			.FileSystem;

		fileSystem.File.WriteAllText("foo.txt", "bar");

		await Expect.That(fileSystem.DriveInfo.GetDrives()).Contains(d => d.Name == "U:\\");
		await Expect.That(fileSystem.File.Exists("U:\\sub\\directory\\foo.txt")).IsTrue();
	}

	[Fact]
	public async Task DefineAdditionalDrive()
	{
		MockFileSystem fileSystem = new MockFileSystem()
			.WithDrive("T:\\");

		fileSystem.File.WriteAllText("T:\\foo.txt", "bar");

		await Expect.That(fileSystem.DriveInfo.GetDrives()).Contains(d => d.Name == "T:\\");
		await Expect.That(fileSystem.File.Exists("T:\\foo.txt")).IsTrue();
	}

	[Fact]
	public async Task LimitAvailableDriveSize()
	{
		MockFileSystem fileSystem = new MockFileSystem()
			.WithDrive("C:\\", d => d.SetTotalSize(100));
		IDriveInfo drive = fileSystem.GetDefaultDrive();
		byte[] largeFileContent = new byte[90];
		Random.Shared.NextBytes(largeFileContent);

		await Expect.That(drive.AvailableFreeSpace).IsEqualTo(100);
		fileSystem.File.WriteAllText("foo.txt", "This is a text with 29 bytes.");
		await Expect.That(drive.AvailableFreeSpace).IsEqualTo(71);
		fileSystem.File.AppendAllText("foo.txt", "Another 17 bytes.");
		await Expect.That(drive.AvailableFreeSpace).IsEqualTo(54);

		void Act()
		{
			fileSystem.File.WriteAllBytes("bar.bin", largeFileContent);
		}

		await Expect.That(Act).Throws<IOException>();

		await Expect.That(drive.AvailableFreeSpace).IsEqualTo(54);
		fileSystem.File.Delete("foo.txt");
		await Expect.That(drive.AvailableFreeSpace).IsEqualTo(100);
		fileSystem.File.WriteAllBytes("bar.bin", largeFileContent);
		await Expect.That(drive.AvailableFreeSpace).IsEqualTo(10);
	}
}
