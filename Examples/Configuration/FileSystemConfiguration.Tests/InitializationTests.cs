using aweXpect;
using System.IO;
using System.IO.Abstractions;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
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
	[InlineData("foo")]
	public async Task InitializeFileSystemInSpecifiedCurrentDirectory(string currentDirectory)
	{
		MockFileSystem fileSystem = new();
		string expectedDirectory = fileSystem.Path.GetFullPath(currentDirectory);

		fileSystem.InitializeIn(currentDirectory)
			.WithASubdirectory();

		await Expect.That(fileSystem.Directory.GetCurrentDirectory()).IsEqualTo(expectedDirectory);
	}

	/// <summary>
	///     Initialize the file system in the root directory with<br />
	///     - a randomly named directory
	///     - a directory named "foo" which contains a randomly named file
	///     - a file named "bar.txt"
	/// </summary>
	[Fact]
	public async Task InitializeFileSystemInTheRootDirectory()
	{
		MockFileSystem fileSystem = new();
		fileSystem.InitializeIn("base-directory")
			.WithASubdirectory()
			.WithSubdirectory("foo")
			.Initialized(s => s
				.WithAFile())
			.WithFile("bar.txt");

		await Expect.That(fileSystem.File.Exists("bar.txt")).IsTrue();
		await Expect.That(fileSystem.Directory.Exists("foo")).IsTrue();
		await Expect.That(fileSystem.Directory.GetDirectories(".")).HasCount(2);
	}

	/// <summary>
	///     The file system is automatically initialized with the main drive
	///     (`C:` on Windows, `/` on Linux or Mac).<br />
	///     UNC servers (or additional drives under windows) can be added if required.
	/// </summary>
	[Fact]
	public async Task InitializeFileSystemWithUncDrive()
	{
		MockFileSystem fileSystem = new();
		int initialDriveCount = fileSystem.DriveInfo.GetDrives().Length;

		fileSystem.WithUncDrive(@"//unc-server");

		await Expect.That(fileSystem.DriveInfo.GetDrives()).HasCount(initialDriveCount);
		IDriveInfo drive = fileSystem.DriveInfo.New(@"//unc-server");
		await Expect.That(drive.IsReady).IsTrue();

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			await Expect.That(fileSystem.DriveInfo.GetDrives()).HasCount(initialDriveCount);
		}
	}

	/// <summary>
	///     The drives can be configured on the <see cref="MockFileSystem" />.
	///     Per default all drives are initializes with 1GB of free space. All
	///     file operations are counted against this limit and throw an
	///     <see cref="IOException" />, when the limit is breached.
	/// </summary>
	[Fact]
	public async Task LimitAvailableSpaceOnDrives()
	{
		MockFileSystem fileSystem = new();
		IRandom random = fileSystem.RandomSystem.Random.Shared;
		byte[] firstFileContent = new byte[199];
		byte[] secondFileContent = new byte[2];
		random.NextBytes(firstFileContent);
		random.NextBytes(secondFileContent);

		// Limit the main drive to 200 bytes
		fileSystem.WithDrive(drive => drive.SetTotalSize(200));
		IDriveInfo mainDrive = fileSystem.GetDefaultDrive();
		await Expect.That(mainDrive.AvailableFreeSpace).IsEqualTo(200);

		fileSystem.File.WriteAllBytes("foo", firstFileContent);
		await Expect.That(mainDrive.AvailableFreeSpace).IsEqualTo(1);

		void Act()
		{
			fileSystem.File.WriteAllBytes("bar", secondFileContent);
		}

		await Expect.That(Act).Throws<IOException>();
	}
}
