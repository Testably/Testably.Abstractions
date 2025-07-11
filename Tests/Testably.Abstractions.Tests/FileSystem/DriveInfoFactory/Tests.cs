using System.IO;
using System.Linq;

namespace Testably.Abstractions.Tests.FileSystem.DriveInfoFactory;

[FileSystemTests]
public partial class Tests
{
	[Fact]
	public async Task GetDrives_ShouldNotBeEmpty()
	{
		IDriveInfo[] result = FileSystem.DriveInfo.GetDrives();

		await That(result).IsNotEmpty();
	}

	[Theory]
	[AutoData]
	public async Task MissingDrive_CreateDirectoryInfo_ShouldOnlyThrowWhenAccessingData(
		string path, string subPath)
	{
		Skip.IfNot(Test.RunsOnWindows);

		IDriveInfo driveInfo = GetUnmappedDrive();

		path = $"{driveInfo.Name}{path}";
		IDirectoryInfo directoryInfo =
			FileSystem.DirectoryInfo.New(FileSystem.Path.Combine(path, subPath));
		IDirectoryInfo? parent = directoryInfo.Parent;

		void Act()
		{
			_ = parent!.EnumerateDirectories().ToArray();
		}

		await That(Act).Throws<DirectoryNotFoundException>()
			.WithMessageContaining($"'{path}'").And
			.WithHResult(-2147024893);
	}

	[Theory]
	[AutoData]
	public async Task MissingDrive_WriteAllBytes_ShouldThrowDirectoryNotFoundException(
		string path, byte[] bytes)
	{
		Skip.IfNot(Test.RunsOnWindows);

		IDriveInfo driveInfo = GetUnmappedDrive();

		path = $"{driveInfo.Name}{path}";

		void Act()
		{
			FileSystem.File.WriteAllBytes(path, bytes);
		}

		await That(Act).Throws<DirectoryNotFoundException>()
			.WithMessageContaining($"'{path}'").And
			.WithHResult(-2147024893);
	}

	[Fact]
	public async Task New_DefaultDrive_ShouldBeFixed()
	{
		IDriveInfo result =
			FileSystem.DriveInfo.New(FileTestHelper.RootDrive(Test));

		await That(result.AvailableFreeSpace).IsGreaterThan(0);
		await That(result.DriveFormat).IsNotNull();
		await That(result.DriveType).IsEqualTo(DriveType.Fixed);
		await That(result.IsReady).IsTrue();
		await That(result.RootDirectory.FullName).IsEqualTo(FileTestHelper.RootDrive(Test));
		await That(result.TotalFreeSpace).IsGreaterThan(0);
		await That(result.TotalSize).IsGreaterThan(0);
		await That(result.VolumeLabel).IsNotEmpty();
	}

	[Theory]
	[AutoData]
	public async Task New_InvalidDriveName_ShouldThrowArgumentException(
		string invalidDriveName)
	{
		Skip.IfNot(Test.RunsOnWindows, "Linux does not support different drives.");

		void Act()
		{
			_ = FileSystem.DriveInfo.New(invalidDriveName);
		}

		await That(Act).Throws<ArgumentException>().WithHResult(-2147024809);
	}

	[Theory]
	[InlineData('A')]
	[InlineData('C')]
	[InlineData('X')]
	public async Task New_WithDriveLetter_ShouldReturnDriveInfo(char driveLetter)
	{
		Skip.IfNot(Test.RunsOnWindows, "Linux does not support different drives.");

		IDriveInfo result = FileSystem.DriveInfo.New($"{driveLetter}");

		await That(result.Name).IsEqualTo($"{driveLetter}:\\");
	}

	[Theory]
	[InlineAutoData('A')]
	[InlineAutoData('C')]
	[InlineAutoData('Y')]
	public async Task New_WithRootedPath_ShouldReturnDriveInfo(char driveLetter, string path)
	{
		Skip.IfNot(Test.RunsOnWindows, "Linux does not support different drives.");

		string rootedPath = FileTestHelper.RootDrive(Test, path, driveLetter);

		IDriveInfo result = FileSystem.DriveInfo.New(rootedPath);

		await That(result.Name).IsEqualTo($"{driveLetter}:\\");
	}

	[Fact]
	public async Task Wrap_Null_ShouldReturnNull()
	{
		Skip.If(FileSystem is MockFileSystem mockFileSystem &&
		        mockFileSystem.SimulationMode != SimulationMode.Native);

		IDriveInfo? result = FileSystem.DriveInfo.Wrap(null);

		await That(result).IsNull();
	}

	[Fact]
	public async Task Wrap_ShouldReturnDriveInfoWithSameName()
	{
		Skip.If(FileSystem is MockFileSystem mockFileSystem &&
		        mockFileSystem.SimulationMode != SimulationMode.Native);

		System.IO.DriveInfo driveInfo = System.IO.DriveInfo.GetDrives()[0];

		IDriveInfo result = FileSystem.DriveInfo.Wrap(driveInfo);

		await That(result.Name).IsEqualTo(driveInfo.Name);
	}

	[Fact]
	public async Task Wrap_WithSimulatedMockFileSystem_ShouldThrowNotSupportedException()
	{
		Skip.IfNot(FileSystem is MockFileSystem mockFileSystem &&
		           mockFileSystem.SimulationMode != SimulationMode.Native);

		System.IO.DriveInfo driveInfo = System.IO.DriveInfo.GetDrives()[0];

		void Act()
		{
			_ = FileSystem.DriveInfo.Wrap(driveInfo);
		}

		await That(Act).ThrowsExactly<NotSupportedException>().Whose(x => x.Message,
			it => it.Contains("Wrapping a DriveInfo in a simulated file system is not supported"));
	}

	#region Helpers

	private IDriveInfo GetUnmappedDrive()
	{
		IDriveInfo? driveInfo = null;
		for (char c = 'A'; c <= 'Z'; c++)
		{
			driveInfo = FileSystem.DriveInfo.New($"{c}");
			if (FileSystem.DriveInfo.GetDrives()
				.All(d => !string.Equals(d.Name, driveInfo.Name,
					StringComparison.OrdinalIgnoreCase)))
			{
				break;
			}
		}

		return driveInfo ?? throw new NotSupportedException("No unmapped drive found!");
	}

	#endregion
}
