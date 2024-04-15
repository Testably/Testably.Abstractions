using System.IO;
using System.Linq;

namespace Testably.Abstractions.Tests.FileSystem.DriveInfoFactory;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class Tests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableFact]
	public void GetDrives_ShouldNotBeEmpty()
	{
		IDriveInfo[] result = FileSystem.DriveInfo.GetDrives();

		result.Should().NotBeEmpty();
	}

	[SkippableTheory]
	[AutoData]
	public void MissingDrive_CreateDirectoryInfo_ShouldOnlyThrowWhenAccessingData(
		string path, string subPath)
	{
		Skip.IfNot(Test.RunsOnWindows);

		IDriveInfo driveInfo = GetUnmappedDrive();

		path = $"{driveInfo.Name}{path}";
		IDirectoryInfo directoryInfo =
			FileSystem.DirectoryInfo.New(FileSystem.Path.Combine(path, subPath));
		IDirectoryInfo? parent = directoryInfo.Parent;

		Exception? exception = Record.Exception(() =>
		{
			_ = parent!.EnumerateDirectories().ToArray();
		});

		exception.Should().BeException<DirectoryNotFoundException>($"'{path}'",
			hResult: -2147024893);
	}

	[SkippableTheory]
	[AutoData]
	public void MissingDrive_WriteAllBytes_ShouldThrowDirectoryNotFoundException(
		string path, byte[] bytes)
	{
		Skip.IfNot(Test.RunsOnWindows);

		IDriveInfo driveInfo = GetUnmappedDrive();

		path = $"{driveInfo.Name}{path}";

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.WriteAllBytes(path, bytes);
		});

		exception.Should().BeException<DirectoryNotFoundException>($"'{path}'",
			hResult: -2147024893);
	}

	[SkippableFact]
	public void New_DefaultDrive_ShouldBeFixed()
	{
		IDriveInfo result =
			FileSystem.DriveInfo.New(FileTestHelper.RootDrive(Test));

		result.AvailableFreeSpace.Should().BeGreaterThan(0);
		result.DriveFormat.Should().NotBeNull();
		result.DriveType.Should().Be(DriveType.Fixed);
		result.IsReady.Should().BeTrue();
		result.RootDirectory.FullName.Should().Be(FileTestHelper.RootDrive(Test));
		result.TotalFreeSpace.Should().BeGreaterThan(0);
		result.TotalSize.Should().BeGreaterThan(0);
		result.VolumeLabel.Should().NotBeEmpty();
	}

	[SkippableTheory]
	[AutoData]
	public void New_InvalidDriveName_ShouldThrowArgumentException(
		string invalidDriveName)
	{
		Skip.IfNot(Test.RunsOnWindows, "Linux does not support different drives.");

		Exception? exception = Record.Exception(() =>
		{
			_ = FileSystem.DriveInfo.New(invalidDriveName);
		});

		exception.Should().BeException<ArgumentException>(hResult: -2147024809);
	}

	[SkippableTheory]
	[InlineData('A')]
	[InlineData('C')]
	[InlineData('X')]
	public void New_WithDriveLetter_ShouldReturnDriveInfo(char driveLetter)
	{
		Skip.IfNot(Test.RunsOnWindows, "Linux does not support different drives.");

		IDriveInfo result = FileSystem.DriveInfo.New($"{driveLetter}");

		result.Name.Should().Be($"{driveLetter}:\\");
	}

	[SkippableTheory]
	[InlineAutoData('A')]
	[InlineAutoData('C')]
	[InlineAutoData('Y')]
	public void New_WithRootedPath_ShouldReturnDriveInfo(char driveLetter, string path)
	{
		Skip.IfNot(Test.RunsOnWindows, "Linux does not support different drives.");

		string rootedPath = FileTestHelper.RootDrive(Test, path, driveLetter);

		IDriveInfo result = FileSystem.DriveInfo.New(rootedPath);

		result.Name.Should().Be($"{driveLetter}:\\");
	}

	[SkippableFact]
	public void Wrap_Null_ShouldReturnNull()
	{
		Skip.If(FileSystem is MockFileSystem mockFileSystem &&
		        mockFileSystem.SimulationMode != SimulationMode.Native);

		IDriveInfo? result = FileSystem.DriveInfo.Wrap(null);

		result.Should().BeNull();
	}

	[SkippableFact]
	public void Wrap_ShouldReturnDriveInfoWithSameName()
	{
		Skip.If(FileSystem is MockFileSystem mockFileSystem &&
		        mockFileSystem.SimulationMode != SimulationMode.Native);

		System.IO.DriveInfo driveInfo = System.IO.DriveInfo.GetDrives().First();

		IDriveInfo result = FileSystem.DriveInfo.Wrap(driveInfo);

		result.Name.Should().Be(driveInfo.Name);
	}

	[SkippableFact]
	public void Wrap_WithSimulatedMockFileSystem_ShouldThrowNotSupportedException()
	{
		Skip.IfNot(FileSystem is MockFileSystem mockFileSystem &&
		        mockFileSystem.SimulationMode != SimulationMode.Native);

		System.IO.DriveInfo driveInfo = System.IO.DriveInfo.GetDrives().First();

		Exception? exception = Record.Exception(() =>
		{
			_ = FileSystem.DriveInfo.Wrap(driveInfo);
		});

		exception.Should().BeOfType<NotSupportedException>().Which
			.Message.Should().Contain("Wrapping a DriveInfo in a simulated file system is not supported");
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
