using System.IO;
using System.Linq;
using Testably.Abstractions.FileSystem;
using Testably.Abstractions.Testing.FileSystem;
using Testably.Abstractions.Testing.Internal;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.FileSystemMock;

public class MockFileSystemTests
{
	[SkippableTheory]
	[AutoData]
	public void FileSystemMock_File_Decrypt(string path, string contents)
	{
		MockFileSystem sut = new();
		sut.File.WriteAllText(path, contents);
#pragma warning disable CA1416
		sut.File.Encrypt(path);
#pragma warning restore CA1416

#pragma warning disable CA1416
		sut.File.Decrypt(path);
#pragma warning restore CA1416

		sut.File.ReadAllText(path).Should().Be(contents);
	}

	[SkippableTheory]
	[AutoData]
	public void FileSystemMock_File_Encrypt(string path, string contents)
	{
		MockFileSystem sut = new();
		sut.File.WriteAllText(path, contents);

#pragma warning disable CA1416
		sut.File.Encrypt(path);
#pragma warning restore CA1416

		sut.File.ReadAllText(path).Should().NotBe(contents);
	}

	[SkippableTheory]
	[AutoData]
	public void FileSystemMock_FileInfo_Decrypt(string path, string contents)
	{
		MockFileSystem sut = new();
		sut.File.WriteAllText(path, contents);
#pragma warning disable CA1416
		sut.FileInfo.New(path).Encrypt();
#pragma warning restore CA1416

#pragma warning disable CA1416
		sut.FileInfo.New(path).Decrypt();
#pragma warning restore CA1416

		sut.File.ReadAllText(path).Should().Be(contents);
	}

	[SkippableTheory]
	[AutoData]
	public void FileSystemMock_FileInfo_Encrypt(string path, string contents)
	{
		MockFileSystem sut = new();
		sut.File.WriteAllText(path, contents);

#pragma warning disable CA1416
		sut.FileInfo.New(path).Encrypt();
#pragma warning restore CA1416

		sut.File.ReadAllText(path).Should().NotBe(contents);
	}

	[SkippableFact]
	public void FileSystemMock_ShouldBeInitializedWithASingleDefaultDrive()
	{
		string expectedDriveName = "".PrefixRoot();
		MockFileSystem sut = new();

		IDriveInfo[] drives = sut.DriveInfo.GetDrives();
		IDriveInfo drive = drives.Single();

		drive.Name.Should().Be(expectedDriveName);
		drive.AvailableFreeSpace.Should().BeGreaterThan(0);
		drive.DriveFormat.Should()
		   .Be(DriveInfoMock.DefaultDriveFormat);
		drive.DriveType.Should()
		   .Be(DriveInfoMock.DefaultDriveType);
		drive.VolumeLabel.Should().NotBeNullOrEmpty();
	}

	[SkippableTheory]
	[AutoData]
	public void UncDrive_WriteBytes_ShouldReduceAvailableFreeSpace(
		string server, string path, byte[] bytes)
	{
		MockFileSystem sut = new();
		string uncPrefix = new(sut.Path.DirectorySeparatorChar, 2);
		string uncDrive = $"{uncPrefix}{server}";
		sut.WithUncDrive(uncDrive);
		IDriveInfo drive = sut.DriveInfo.New(uncDrive);
		long previousFreeSpace = drive.AvailableFreeSpace;

		sut.File.WriteAllBytes(Path.Combine(uncDrive, path), bytes);

		drive.AvailableFreeSpace.Should().Be(previousFreeSpace - bytes.Length);
	}

	[SkippableTheory]
	[AutoData]
	public void WithAccessControl_Denied_CreateDirectoryShouldThrowIOException(
		string path)
	{
		MockFileSystem sut = new();
		sut.Initialize();
		sut.WithAccessControl((_, _) => false);

		Exception? exception = Record.Exception(() =>
		{
			sut.Directory.CreateDirectory(path);
		});

		exception.Should().BeOfType<IOException>();
	}

	[SkippableTheory]
	[AutoData]
	public void WithAccessControl_ShouldConsiderPath(
		string allowedPath, string deniedPath)
	{
		MockFileSystem sut = new();
		sut.Initialize();
		sut.WithAccessControl((p, _) => p == sut.Path.GetFullPath(allowedPath));

		sut.Directory.CreateDirectory(allowedPath);
		Exception? exception = Record.Exception(() =>
		{
			sut.Directory.CreateDirectory(deniedPath);
		});

		exception.Should().BeOfType<IOException>();
	}

	[SkippableFact]
	public void WithDrive_ExistingName_ShouldUpdateDrive()
	{
		string driveName = "".PrefixRoot();
		MockFileSystem sut = new();
		sut.WithDrive(driveName);

		IDriveInfo[] drives = sut.DriveInfo.GetDrives();

		drives.Length.Should().Be(1);
		drives.Should().ContainSingle(d => d.Name == driveName);
	}

	[SkippableTheory]
	[InlineData("D:\\")]
	public void WithDrive_NewName_ShouldCreateNewDrives(string driveName)
	{
		Skip.IfNot(Test.RunsOnWindows, "Linux does not support different drives.");
		MockFileSystem sut = new();
		sut.WithDrive(driveName);

		IDriveInfo[] drives = sut.DriveInfo.GetDrives();

		drives.Length.Should().Be(2);
		drives.Should().ContainSingle(d => d.Name == driveName);
	}

	[SkippableTheory]
	[AutoData]
	public void WithDrive_WithCallback_ShouldUpdateDrive(long totalSize)
	{
		MockFileSystem sut = new();
		sut.WithDrive(d => d.SetTotalSize(totalSize));

		IDriveInfo drive = sut.DriveInfo.GetDrives().Single();

		drive.TotalSize.Should().Be(totalSize);
		drive.TotalFreeSpace.Should().Be(totalSize);
		drive.AvailableFreeSpace.Should().Be(totalSize);
	}

	[SkippableTheory]
	[AutoData]
	public void WithUncDrive_ShouldCreateUncDrive(
		string path, string contents)
	{
		MockFileSystem sut = new();
		sut.WithUncDrive("UNC-Path");
		string fullPath = sut.Path.Combine("//UNC-Path", path);
		sut.File.WriteAllText(fullPath, contents);

		string result = sut.File.ReadAllText(fullPath);
		result.Should().Be(contents);
	}

	[SkippableTheory]
	[AutoData]
	public void WriteAllText_OnUncPath_ShouldThrowDirectoryNotFoundException(
		string path, string contents)
	{
		MockFileSystem sut = new();
		string fullPath = sut.Path.Combine("//UNC-Path", path);
		Exception? exception = Record.Exception(() =>
		{
			sut.File.WriteAllText(fullPath, contents);
		});

		exception.Should().BeOfType<DirectoryNotFoundException>();
	}
}