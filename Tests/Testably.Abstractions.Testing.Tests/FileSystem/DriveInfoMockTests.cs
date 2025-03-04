using System.IO;
using System.Text;
using Testably.Abstractions.Testing.FileSystem;

namespace Testably.Abstractions.Testing.Tests.FileSystem;

public class DriveInfoMockTests
{
	#region Test Setup

	public MockFileSystem FileSystem { get; } = new();

	#endregion

	[Theory]
	[AutoData]
	public void AvailableFreeSpace_CannotGetNegative(long size)
	{
		FileSystem.WithDrive(d => d.SetTotalSize(size));
		IDriveInfo drive = FileSystem.GetDefaultDrive();

		FileSystem.WithDrive(d => d.ChangeUsedBytes(-1));

		drive.AvailableFreeSpace.Should().Be(size);
	}

	[Theory]
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

		IDriveInfo drive = FileSystem.GetDefaultDrive();
		exception.Should().BeOfType<IOException>()
			.Which.Message.Should().Contain($"'{drive.Name}'");
		drive.AvailableFreeSpace.Should().Be(fileSize - 1);
	}

	[Theory]
	[AutoData]
	public void AvailableFreeSpace_ShouldBeChangedWhenAppendingToAFile(
		string fileContent1, string fileContent2, int expectedRemainingBytes,
		string path, Encoding encoding)
	{
		int fileSize1 = encoding.GetPreamble().Length + encoding.GetBytes(fileContent1).Length;
		int fileSize2 = encoding.GetBytes(fileContent2).Length;
		FileSystem.WithDrive(d
			=> d.SetTotalSize(fileSize1 + fileSize2 + expectedRemainingBytes));
		IDriveInfo drive = FileSystem.GetDefaultDrive();

		FileSystem.File.WriteAllText(path, fileContent1, encoding);
		drive.AvailableFreeSpace.Should().Be(expectedRemainingBytes + fileSize2);
		FileSystem.File.AppendAllText(path, fileContent2, encoding);

		drive.AvailableFreeSpace.Should().Be(expectedRemainingBytes);
	}

	[Theory]
	[InlineAutoData(0)]
	[InlineAutoData(1)]
	[InlineAutoData(10)]
	public void AvailableFreeSpace_ShouldBeChangedWhenWorkingWithStreams(
		int reduceLength, string path, string previousContent)
	{
		FileSystem.File.WriteAllText(path, previousContent);
		IDriveInfo drive = FileSystem.GetDefaultDrive();
		long previousFreeSpace = drive.AvailableFreeSpace;

		FileSystemStream stream = FileSystem.File.OpenWrite(path);
		using (StreamWriter streamWriter = new(stream))
		{
			streamWriter.Write("new-content");
			stream.SetLength(stream.Length - reduceLength);
		}

		drive.AvailableFreeSpace.Should().Be(previousFreeSpace + reduceLength);
	}

	[Theory]
	[AutoData]
	public void AvailableFreeSpace_ShouldBeReducedByWritingToFile(
		int fileSize, string path)
	{
		byte[] bytes = new byte[fileSize];
		FileSystem.WithDrive(d => d.SetTotalSize(fileSize));
		FileSystem.RandomSystem.Random.Shared.NextBytes(bytes);

		FileSystem.File.WriteAllBytes(path, bytes);

		IDriveInfo drive = FileSystem.GetDefaultDrive();

		drive.AvailableFreeSpace.Should().Be(0);
	}

	[Theory]
	[AutoData]
	public void AvailableFreeSpace_ShouldBeReleasedWhenDeletingAFile(
		int fileSize, string path)
	{
		byte[] bytes = new byte[fileSize];
		FileSystem.WithDrive(d => d.SetTotalSize(fileSize));
		FileSystem.RandomSystem.Random.Shared.NextBytes(bytes);

		FileSystem.File.WriteAllBytes(path, bytes);
		FileSystem.File.Delete(path);

		IDriveInfo drive = FileSystem.GetDefaultDrive();

		drive.AvailableFreeSpace.Should().Be(fileSize);
	}

	[Theory]
	[AutoData]
	public void AvailableFreeSpace_ShouldBeSetTotalSize(long size)
	{
		FileSystem.WithDrive(d => d.SetTotalSize(size));

		IDriveInfo drive = FileSystem.GetDefaultDrive();

		drive.AvailableFreeSpace.Should().Be(size);
	}

	[Theory]
	[InlineData(@"//foo", @"//foo")]
	[InlineData(@"//foo/bar", @"//foo")]
	[InlineData(@"//foo/bar/xyz", @"//foo")]
	public void New_DriveNameWithUncPath_ShouldUseTopMostDirectory(
		string driveName, string expectedName)
	{
		expectedName = expectedName
			.Replace('/', FileSystem.Path.DirectorySeparatorChar);

		DriveInfoMock drive =
			DriveInfoMock.New(driveName, FileSystem);

		drive.Name.Should().Be(expectedName);
	}

	[Theory]
	[InlineData("foo")]
	public void New_InvalidDriveName_ShouldThrowArgumentException(string driveName)
	{
		Exception? exception = Record.Exception(() =>
		{
			DriveInfoMock.New(driveName, FileSystem);
		});

		exception.Should().BeOfType<ArgumentException>();
	}

	[Fact]
	public void New_Null_ShouldReturnNull()
	{
		IDriveInfo? drive =
			DriveInfoMock.New(null, FileSystem);

		drive.Should().BeNull();
	}

	[Fact]
	public void New_UncPath_ShouldSetFlag()
	{
		IDriveInfo drive =
			DriveInfoMock.New(@"//foo", FileSystem);

		(drive as DriveInfoMock)?.IsUncPath.Should().BeTrue();
	}

	[Theory]
	[InlineData("C", "C:\\")]
	[InlineData("d", "D:\\")]
	public void New_ValidDriveName_ShouldAppendColonAndSlash(
		string driveName, string expectedDriveName)
	{
		DriveInfoMock result =
			DriveInfoMock.New(driveName, FileSystem);

		result.Name.Should().Be(expectedDriveName);
	}

	[Theory]
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

	[Theory]
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

	[Fact]
	public void SetDriveFormat_Default_ShouldBeNTFS()
	{
		FileSystem.WithDrive(d => d.SetDriveFormat());

		IDriveInfo drive = FileSystem.GetDefaultDrive();
		drive.DriveFormat.Should().Be("NTFS");
	}

	[Theory]
	[AutoData]
	public void SetDriveFormat_ShouldChangeDriveFormat(string driveFormat)
	{
		FileSystem.WithDrive(d => d.SetDriveFormat(driveFormat));

		IDriveInfo drive = FileSystem.GetDefaultDrive();
		drive.DriveFormat.Should().Be(driveFormat);
	}

	[Fact]
	public void SetDriveType_Default_ShouldBeFixed()
	{
		FileSystem.WithDrive(d => d.SetDriveType());

		IDriveInfo drive = FileSystem.GetDefaultDrive();
		drive.DriveType.Should().Be(DriveType.Fixed);
	}

	[Theory]
	[AutoData]
	public void SetDriveType_ShouldChangeDriveType(DriveType driveType)
	{
		FileSystem.WithDrive(d => d.SetDriveType(driveType));

		IDriveInfo drive = FileSystem.GetDefaultDrive();
		drive.DriveType.Should().Be(driveType);
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void SetIsReady_ShouldChangeIsReady(bool isReady)
	{
		FileSystem.WithDrive(d => d.SetIsReady(isReady));

		IDriveInfo drive = FileSystem.GetDefaultDrive();
		drive.IsReady.Should().Be(isReady);
	}

	[Fact]
	public void SetTotalSize_Default_ShouldBe1Gigabyte()
	{
		FileSystem.WithDrive(d => d.SetTotalSize());

		IDriveInfo drive = FileSystem.GetDefaultDrive();

		drive.AvailableFreeSpace.Should().Be(1024 * 1024 * 1024);
	}
}
