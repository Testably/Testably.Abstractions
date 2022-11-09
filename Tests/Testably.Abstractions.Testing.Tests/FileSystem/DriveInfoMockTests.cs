using System.IO;
using System.Linq;
using System.Text;
using Testably.Abstractions.FileSystem;
using Testably.Abstractions.Testing.FileSystem;

namespace Testably.Abstractions.Testing.Tests.FileSystem;

public class DriveInfoMockTests
{
	public MockFileSystem FileSystem { get; }

	public DriveInfoMockTests()
	{
		FileSystem = new MockFileSystem();
	}

	[SkippableTheory]
	[AutoData]
	public void AvailableFreeSpace_CannotGetNegative(long size)
	{
		FileSystem.WithDrive(d => d.SetTotalSize(size));
		IDriveInfo drive = FileSystem.DriveInfo.GetDrives().Single();

		FileSystem.WithDrive(d => d.ChangeUsedBytes(-1));

		drive.AvailableFreeSpace.Should().Be(size);
	}

	[SkippableTheory]
	[AutoData]
	public void AvailableFreeSpace_NotEnoughSpace_ShouldThrowIOException(
		int fileSize, string path)
	{
		byte[] bytes = new byte[fileSize];
		FileSystem.WithDrive(d => d.SetTotalSize(fileSize - 1));
		FileSystem.RandomSystem.Random.Shared.NextBytes(bytes);

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.WriteAllBytes(path, bytes);
		});

		IDriveInfo drive = FileSystem.DriveInfo.GetDrives().Single();
		exception.Should().BeOfType<IOException>()
		   .Which.Message.Should().Contain($"'{drive.Name}'");
		drive.AvailableFreeSpace.Should().Be(fileSize - 1);
	}

	[SkippableTheory]
	[AutoData]
	public void AvailableFreeSpace_ShouldBeChangedWhenAppendingToAFile(
		string fileContent1, string fileContent2, int expectedRemainingBytes,
		string path, Encoding encoding)
	{
		int fileSize1 = encoding.GetPreamble().Length + encoding.GetBytes(fileContent1).Length;
		int fileSize2 = encoding.GetBytes(fileContent2).Length;
		FileSystem.WithDrive(d
			=> d.SetTotalSize(fileSize1 + fileSize2 + expectedRemainingBytes));
		IDriveInfo drive = FileSystem.DriveInfo.GetDrives().Single();

		FileSystem.File.WriteAllText(path, fileContent1, encoding);
		drive.AvailableFreeSpace.Should().Be(expectedRemainingBytes + fileSize2);
		FileSystem.File.AppendAllText(path, fileContent2, encoding);

		drive.AvailableFreeSpace.Should().Be(expectedRemainingBytes);
	}

	[SkippableTheory]
	[AutoData]
	public void AvailableFreeSpace_ShouldBeReducedByWritingToFile(
		int fileSize, string path)
	{
		byte[] bytes = new byte[fileSize];
		FileSystem.WithDrive(d => d.SetTotalSize(fileSize));
		FileSystem.RandomSystem.Random.Shared.NextBytes(bytes);

		FileSystem.File.WriteAllBytes(path, bytes);

		IDriveInfo drive = FileSystem.DriveInfo.GetDrives().Single();

		drive.AvailableFreeSpace.Should().Be(0);
	}

	[SkippableTheory]
	[AutoData]
	public void AvailableFreeSpace_ShouldBeReleasedWhenDeletingAFile(
		int fileSize, string path)
	{
		byte[] bytes = new byte[fileSize];
		FileSystem.WithDrive(d => d.SetTotalSize(fileSize));
		FileSystem.RandomSystem.Random.Shared.NextBytes(bytes);

		FileSystem.File.WriteAllBytes(path, bytes);
		FileSystem.File.Delete(path);

		IDriveInfo drive = FileSystem.DriveInfo.GetDrives().Single();

		drive.AvailableFreeSpace.Should().Be(fileSize);
	}

	[SkippableTheory]
	[AutoData]
	public void AvailableFreeSpace_ShouldBeSetTotalSize(long size)
	{
		FileSystem.WithDrive(d => d.SetTotalSize(size));

		IDriveInfo drive = FileSystem.DriveInfo.GetDrives().Single();

		drive.AvailableFreeSpace.Should().Be(size);
	}

	[SkippableTheory]
	[InlineData(@"//foo", @"//foo")]
	[InlineData(@"//foo/bar", @"//foo")]
	[InlineData(@"//foo/bar/xyz", @"//foo")]
	public void New_DriveNameWithUncPath_ShouldUseTopMostDirectory(
		string driveName, string expectedName)
	{
		expectedName = expectedName
		   .Replace('/', FileSystem.Path.DirectorySeparatorChar);

		IDriveInfo drive =
			DriveInfoMock.New(driveName, FileSystem);

		drive.Name.Should().Be(expectedName);
	}

	[SkippableTheory]
	[InlineData("foo")]
	public void New_InvalidDriveName_ShouldThrowArgumentException(string driveName)
	{
		Exception? exception = Record.Exception(() =>
		{
			DriveInfoMock.New(driveName, FileSystem);
		});

		exception.Should().BeOfType<ArgumentException>();
	}

	[SkippableFact]
	public void New_Null_ShouldReturnNull()
	{
		IDriveInfo? drive =
			DriveInfoMock.New(null, FileSystem);

		drive.Should().BeNull();
	}

	[SkippableFact]
	public void New_UncPath_ShouldSetFlag()
	{
		IDriveInfo drive =
			DriveInfoMock.New(@"//foo", FileSystem);

		(drive as DriveInfoMock)?.IsUncPath.Should().BeTrue();
	}

	[SkippableTheory]
	[InlineData("C", "C:\\")]
	[InlineData("d", "D:\\")]
	public void New_ValidDriveName_ShouldAppendColonAndSlash(
		string driveName, string expectedDriveName)
	{
		DriveInfoMock result =
			DriveInfoMock.New(driveName, FileSystem);

		result.Name.Should().Be(expectedDriveName);
	}

	[SkippableTheory]
	[AutoData]
	public void NotReady_AccessDirectory_ShouldThrowIOException(
		string path)
	{
		FileSystem.WithDrive(d => d.SetIsReady(false));

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.CreateDirectory(path);
		});

		exception.Should().BeOfType<IOException>();
	}

	[SkippableTheory]
	[AutoData]
	public void NotReady_AccessFile_ShouldThrowIOException(
		string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);
		FileSystem.WithDrive(d => d.SetIsReady(false));

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.ReadAllText(path);
		});

		exception.Should().BeOfType<IOException>();
	}

	[SkippableFact]
	public void SetDriveFormat_Default_ShouldBeNTFS()
	{
		FileSystem.WithDrive(d => d.SetDriveFormat());

		IDriveInfo drive = FileSystem.DriveInfo.GetDrives().Single();
		drive.DriveFormat.Should().Be("NTFS");
	}

	[SkippableTheory]
	[AutoData]
	public void SetDriveFormat_ShouldChangeDriveFormat(string driveFormat)
	{
		FileSystem.WithDrive(d => d.SetDriveFormat(driveFormat));

		IDriveInfo drive = FileSystem.DriveInfo.GetDrives().Single();
		drive.DriveFormat.Should().Be(driveFormat);
	}

	[SkippableFact]
	public void SetDriveType_Default_ShouldBeFixed()
	{
		FileSystem.WithDrive(d => d.SetDriveType());

		IDriveInfo drive = FileSystem.DriveInfo.GetDrives().Single();
		drive.DriveType.Should().Be(DriveType.Fixed);
	}

	[SkippableTheory]
	[AutoData]
	public void SetDriveType_ShouldChangeDriveType(DriveType driveType)
	{
		FileSystem.WithDrive(d => d.SetDriveType(driveType));

		IDriveInfo drive = FileSystem.DriveInfo.GetDrives().Single();
		drive.DriveType.Should().Be(driveType);
	}

	[SkippableTheory]
	[InlineData(true)]
	[InlineData(false)]
	public void SetIsReady_ShouldChangeIsReady(bool isReady)
	{
		FileSystem.WithDrive(d => d.SetIsReady(isReady));

		IDriveInfo drive = FileSystem.DriveInfo.GetDrives().Single();
		drive.IsReady.Should().Be(isReady);
	}

	[SkippableFact]
	public void SetTotalSize_Default_ShouldBe1Gigabyte()
	{
		FileSystem.WithDrive(d => d.SetTotalSize());

		IDriveInfo drive = FileSystem.DriveInfo.GetDrives().Single();

		drive.AvailableFreeSpace.Should().Be(1024 * 1024 * 1024);
	}
}