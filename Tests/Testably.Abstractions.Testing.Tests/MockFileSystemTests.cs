using System.IO;
using System.Linq;
using Testably.Abstractions.Testing.FileSystem;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.Testing.Tests.TestHelpers;
#if NET6_0_OR_GREATER
using Microsoft.Win32.SafeHandles;
#endif

namespace Testably.Abstractions.Testing.Tests;

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
	public void FileSystemMock_ShouldBeInitializedWithADefaultDrive()
	{
		MockFileSystem sut = new();
		string expectedDriveName = "".PrefixRoot(sut);

		IDriveInfo[] drives = sut.DriveInfo.GetDrives();
		IDriveInfo drive = sut.GetDefaultDrive();

		drives.Should().NotBeEmpty();
		drive.Name.Should().Be(expectedDriveName);
		drive.AvailableFreeSpace.Should().BeGreaterThan(0);
		drive.DriveFormat.Should()
			.Be(DriveInfoMock.DefaultDriveFormat);
		drive.DriveType.Should()
			.Be(DriveInfoMock.DefaultDriveType);
		drive.VolumeLabel.Should().NotBeNullOrEmpty();
	}

	[SkippableFact]
	public void FileSystemMock_ShouldInitializeDriveFromCurrentDirectory()
	{
		string? driveName = Path.GetPathRoot(Directory.GetCurrentDirectory());

		Skip.If(!Test.RunsOnWindows || driveName?.StartsWith('C') != false);

		MockFileSystem sut = new();

		IDriveInfo[] drives = sut.DriveInfo.GetDrives();
		drives.Length.Should().Be(2);
		drives.Should().Contain(d => d.Name == driveName);
	}

	[SkippableFact]
	public void ToString_ShouldContainStorageInformation()
	{
		MockFileSystem sut = new();
		sut.File.WriteAllText("foo", "bar");

		string result = sut.ToString();

		result.Should().Contain("directories: 0, files: 1");
	}

	[SkippableTheory]
	[AutoData]
	public void WithAccessControl_Denied_CreateDirectoryShouldThrowIOException(
		string path)
	{
		MockFileSystem sut = new();
		sut.Initialize();
		sut.WithAccessControlStrategy(new DefaultAccessControlStrategy((_, _) => false));

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
		sut.WithAccessControlStrategy(new DefaultAccessControlStrategy((p, _)
			=> string.Equals(p, sut.Path.GetFullPath(allowedPath), StringComparison.Ordinal)));

		sut.Directory.CreateDirectory(allowedPath);
		Exception? exception = Record.Exception(() =>
		{
			sut.Directory.CreateDirectory(deniedPath);
		});

		exception.Should().BeOfType<IOException>();
	}

	[SkippableTheory]
	[InlineData("D:\\")]
	public void WithDrive_Duplicate_ShouldUpdateExistingDrive(string driveName)
	{
		Skip.IfNot(Test.RunsOnWindows, "Linux does not support different drives.");

		MockFileSystem sut = new();
		sut.WithDrive(driveName, d => d.SetTotalSize(100));
		sut.DriveInfo.GetDrives().Length.Should().Be(2);
		IDriveInfo drive = sut.DriveInfo.GetDrives()
			.Single(x => string.Equals(x.Name, driveName, StringComparison.Ordinal));
		drive.TotalSize.Should().Be(100);

		sut.WithDrive(driveName, d => d.SetTotalSize(200));
		sut.DriveInfo.GetDrives().Length.Should().Be(2);
		drive.TotalSize.Should().Be(200);
	}

	[SkippableFact]
	public void WithDrive_ExistingName_ShouldUpdateDrive()
	{
		MockFileSystem sut = new();
		string driveName = "".PrefixRoot(sut);
		sut.WithDrive(driveName);

		IDriveInfo[] drives = sut.DriveInfo.GetDrives();

		drives.Length.Should().BeGreaterOrEqualTo(1);
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
	[InlineData("D")]
	[InlineData("D:")]
	public void WithDrive_ShouldHavePathSeparatorSuffix(string driveName)
	{
		Skip.IfNot(Test.RunsOnWindows, "Linux does not support different drives.");

		string expectedDriveName = $"D:{Path.DirectorySeparatorChar}";
		MockFileSystem sut = new();
		sut.WithDrive(driveName);

		IDriveInfo[] drives = sut.DriveInfo.GetDrives();

		drives.Length.Should().BeLessOrEqualTo(2);
		drives.Should().ContainSingle(d => d.Name == expectedDriveName);
	}

	[SkippableTheory]
	[AutoData]
	public void WithDrive_WithCallback_ShouldUpdateDrive(long totalSize)
	{
		MockFileSystem sut = new();
		sut.WithDrive(d => d.SetTotalSize(totalSize));

		IDriveInfo drive = sut.GetDefaultDrive();

		drive.TotalSize.Should().Be(totalSize);
		drive.TotalFreeSpace.Should().Be(totalSize);
		drive.AvailableFreeSpace.Should().Be(totalSize);
	}

#if NET6_0_OR_GREATER
	[SkippableTheory]
	[AutoData]
	public void
		WithSafeFileHandleStrategy_DefaultStrategy_ShouldUseMappedSafeFileHandleMock(
			string path, string contents)
	{
		MockFileSystem sut = new();
		sut.File.WriteAllText(path, contents);
		sut.WithSafeFileHandleStrategy(
			new DefaultSafeFileHandleStrategy(_ => new SafeFileHandleMock(path)));

		using FileSystemStream stream =
			sut.FileStream.New(new SafeFileHandle(), FileAccess.Read);
		using StreamReader streamReader = new(stream);
		string result = streamReader.ReadToEnd();

		result.Should().Be(contents);
	}
#endif

#if NET6_0_OR_GREATER
	[SkippableTheory]
	[AutoData]
	public void WithSafeFileHandleStrategy_NullStrategy_ShouldThrowException(
		string path, string contents)
	{
		MockFileSystem sut = new();
		sut.File.WriteAllText(path, contents);

		Exception? exception = Record.Exception(() =>
		{
			sut.FileStream.New(new SafeFileHandle(), FileAccess.Read);
		});

		exception.Should().BeOfType<ArgumentException>()
			.Which.ParamName.Should().Be("handle");
	}
#endif

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
	public void WithUncDrive_ShouldNotBeIncludedInGetDrives(
		string server)
	{
		MockFileSystem sut = new();
		string uncPrefix = new(sut.Path.DirectorySeparatorChar, 2);
		string uncDrive = $"{uncPrefix}{server}";
		int expectedLogicalDrives = sut.Directory.GetLogicalDrives().Length;
		int expectedDrives = sut.DriveInfo.GetDrives().Length;

		sut.WithUncDrive(uncDrive);

		sut.Directory.GetLogicalDrives().Length.Should().Be(expectedLogicalDrives);
		sut.DriveInfo.GetDrives().Length.Should().Be(expectedDrives);
	}

	[SkippableTheory]
	[AutoData]
	public void WithUncDrive_WriteBytes_ShouldReduceAvailableFreeSpace(
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
