using System.IO;
using System.Linq;
using Testably.Abstractions.Testing.Internal;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.FileSystemMock;

public class FileSystemMockTests
{
	[Fact]
	public void FileSystemMock_ShouldBeInitializedWithASingleDefaultDrive()
	{
		string expectedDriveName = "".PrefixRoot();
		Testing.FileSystemMock sut = new();

		IFileSystem.IDriveInfo[] drives = sut.DriveInfo.GetDrives();
		IFileSystem.IDriveInfo drive = drives.Single();

		drive.Name.Should().Be(expectedDriveName);
		drive.AvailableFreeSpace.Should().BeGreaterThan(0);
		drive.DriveFormat.Should()
		   .Be(Testing.FileSystemMock.DriveInfoMock.DefaultDriveFormat);
		drive.DriveType.Should()
		   .Be(Testing.FileSystemMock.DriveInfoMock.DefaultDriveType);
		drive.VolumeLabel.Should().NotBeNullOrEmpty();
	}

	[Fact]
	public void WithDrive_ExistingName_ShouldUpdateDrive()
	{
		string driveName = "".PrefixRoot();
		Testing.FileSystemMock sut = new();
		sut.WithDrive(driveName);

		IFileSystem.IDriveInfo[] drives = sut.DriveInfo.GetDrives();

		drives.Length.Should().Be(1);
		drives.Should().ContainSingle(d => d.Name == driveName);
	}

	[Theory]
	[AutoData]
	public void WithDrive_WithCallback_ShouldUpdateDrive(long totalSize)
	{
		Testing.FileSystemMock sut = new();
		sut.WithDrive(d => d.SetTotalSize(totalSize));

		IFileSystem.IDriveInfo drive = sut.DriveInfo.GetDrives().Single();

		drive.TotalSize.Should().Be(totalSize);
		drive.TotalFreeSpace.Should().Be(totalSize);
		drive.AvailableFreeSpace.Should().Be(totalSize);
	}

	[Theory]
	[AutoData]
	public void WithUncDrive_ShouldCreateUncDrive(
		string path, string contents)
	{
		Testing.FileSystemMock sut = new();
		sut.WithUncDrive("UNC-Path");
		string fullPath = sut.Path.Combine("//UNC-Path", path);
		sut.File.WriteAllText(fullPath, contents);

		string result = sut.File.ReadAllText(fullPath);
		result.Should().Be(contents);
	}

	[Theory]
	[AutoData]
	public void UncDrive_WriteBytes_ShouldReduceAvailableFreeSpace(
		string server, string path, byte[] bytes)
	{
		Testing.FileSystemMock sut = new();
		string uncPrefix = new(sut.Path.DirectorySeparatorChar, 2);
		string uncDrive = $"{uncPrefix}{server}";
		sut.WithUncDrive(uncDrive);
		IFileSystem.IDriveInfo drive = sut.DriveInfo.New(uncDrive);
		long previousFreeSpace = drive.AvailableFreeSpace;

		sut.File.WriteAllBytes(Path.Combine(uncDrive, path), bytes);

		drive.AvailableFreeSpace.Should().Be(previousFreeSpace - bytes.Length);
	}

	[Theory]
	[AutoData]
	public void WriteAllText_OnUncPath_ShouldThrowDirectoryNotFoundException(
		string path, string contents)
	{
		Testing.FileSystemMock sut = new();
		string fullPath = sut.Path.Combine("//UNC-Path", path);
		Exception? exception = Record.Exception(() =>
		{
			sut.File.WriteAllText(fullPath, contents);
		});

		exception.Should().BeOfType<DirectoryNotFoundException>();
	}

	#region Helpers

	[SkippableTheory]
	[InlineData("D:\\")]
	public void WithDrive_NewName_ShouldCreateNewDrives(string driveName)
	{
		Skip.IfNot(Test.RunsOnWindows, "Linux does not support different drives.");
		Testing.FileSystemMock sut = new();
		sut.WithDrive(driveName);

		IFileSystem.IDriveInfo[] drives = sut.DriveInfo.GetDrives();

		drives.Length.Should().Be(2);
		drives.Should().ContainSingle(d => d.Name == driveName);
	}

	#endregion
}