using System.IO;
using System.Text;
using Testably.Abstractions.Testing.FileSystem;

namespace Testably.Abstractions.Testing.Tests.FileSystem;

public class DriveInfoMockTests
{
	#region Test Setup

	public MockFileSystem FileSystem { get; } = new();

	#endregion

	[Test]
	[AutoArguments]
	public async Task AvailableFreeSpace_CannotGetNegative(long size)
	{
		FileSystem.WithDrive(d => d.SetTotalSize(size));
		IDriveInfo drive = FileSystem.GetDefaultDrive();

		FileSystem.WithDrive(d => d.ChangeUsedBytes(-1));

		await That(drive.AvailableFreeSpace).IsEqualTo(size);
	}

	[Test]
	[AutoArguments]
	public async Task AvailableFreeSpace_NotEnoughSpace_ShouldThrowIOException(
		int fileSize, string path)
	{
		byte[] bytes = new byte[fileSize];
		FileSystem.WithDrive(d => d.SetTotalSize(fileSize - 1));
		FileSystem.RandomSystem.Random.Shared.NextBytes(bytes);

		void Act()
			=> FileSystem.File.WriteAllBytes(path, bytes);

		IDriveInfo drive = FileSystem.GetDefaultDrive();
		await That(Act).ThrowsExactly<IOException>().WithMessage($"*'{drive.Name}'*").AsWildcard();
		await That(drive.AvailableFreeSpace).IsEqualTo(fileSize - 1);
	}

	[Test]
	[AutoArguments]
	public async Task AvailableFreeSpace_ShouldBeChangedWhenAppendingToAFile(
		string fileContent1, string fileContent2, int expectedRemainingBytes,
		string path, Encoding encoding)
	{
		int fileSize1 = encoding.GetPreamble().Length + encoding.GetBytes(fileContent1).Length;
		int fileSize2 = encoding.GetBytes(fileContent2).Length;
		FileSystem.WithDrive(d
			=> d.SetTotalSize(fileSize1 + fileSize2 + expectedRemainingBytes));
		IDriveInfo drive = FileSystem.GetDefaultDrive();

		FileSystem.File.WriteAllText(path, fileContent1, encoding);
		await That(drive.AvailableFreeSpace).IsEqualTo(expectedRemainingBytes + fileSize2);
		FileSystem.File.AppendAllText(path, fileContent2, encoding);

		await That(drive.AvailableFreeSpace).IsEqualTo(expectedRemainingBytes);
	}

	[Test]
	[AutoArguments(0)]
	[AutoArguments(1)]
	[AutoArguments(10)]
	public async Task AvailableFreeSpace_ShouldBeChangedWhenWorkingWithStreams(
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

		await That(drive.AvailableFreeSpace).IsEqualTo(previousFreeSpace + reduceLength);
	}

	[Test]
	[AutoArguments]
	public async Task AvailableFreeSpace_ShouldBeReducedByWritingToFile(
		int fileSize, string path)
	{
		byte[] bytes = new byte[fileSize];
		FileSystem.WithDrive(d => d.SetTotalSize(fileSize));
		FileSystem.RandomSystem.Random.Shared.NextBytes(bytes);

		FileSystem.File.WriteAllBytes(path, bytes);

		IDriveInfo drive = FileSystem.GetDefaultDrive();

		await That(drive.AvailableFreeSpace).IsEqualTo(0);
	}

	[Test]
	[AutoArguments]
	public async Task AvailableFreeSpace_ShouldBeReleasedWhenDeletingAFile(
		int fileSize, string path)
	{
		byte[] bytes = new byte[fileSize];
		FileSystem.WithDrive(d => d.SetTotalSize(fileSize));
		FileSystem.RandomSystem.Random.Shared.NextBytes(bytes);

		FileSystem.File.WriteAllBytes(path, bytes);
		FileSystem.File.Delete(path);

		IDriveInfo drive = FileSystem.GetDefaultDrive();

		await That(drive.AvailableFreeSpace).IsEqualTo(fileSize);
	}

	[Test]
	[AutoArguments]
	public async Task AvailableFreeSpace_ShouldBeSetTotalSize(long size)
	{
		FileSystem.WithDrive(d => d.SetTotalSize(size));

		IDriveInfo drive = FileSystem.GetDefaultDrive();

		await That(drive.AvailableFreeSpace).IsEqualTo(size);
	}

	[Test]
	[Arguments(@"//foo", @"//foo")]
	[Arguments(@"//foo/bar", @"//foo")]
	[Arguments(@"//foo/bar/xyz", @"//foo")]
	public async Task New_DriveNameWithUncPath_ShouldUseTopMostDirectory(
		string driveName, string expectedName)
	{
		expectedName = expectedName
			.Replace('/', FileSystem.Path.DirectorySeparatorChar);

		DriveInfoMock drive =
			DriveInfoMock.New(driveName, FileSystem);

		await That(drive.Name).IsEqualTo(expectedName);
	}

	[Test]
	[Arguments("foo")]
	public async Task New_InvalidDriveName_ShouldThrowArgumentException(string driveName)
	{
		Exception? exception = Record.Exception(() =>
		{
			DriveInfoMock.New(driveName, FileSystem);
		});

		await That(exception).IsExactly<ArgumentException>();
	}

	[Test]
	public async Task New_Null_ShouldReturnNull()
	{
		IDriveInfo? drive =
			DriveInfoMock.New(null, FileSystem);

		await That(drive).IsNull();
	}

	[Test]
	public async Task New_UncPath_ShouldSetFlag()
	{
		IDriveInfo drive =
			DriveInfoMock.New(@"//foo", FileSystem);

		await That((drive as DriveInfoMock)?.IsUncPath).IsTrue();
	}

	[Test]
	[Arguments("C", "C:\\")]
	[Arguments("d", "D:\\")]
	public async Task New_ValidDriveName_ShouldAppendColonAndSlash(
		string driveName, string expectedDriveName)
	{
		DriveInfoMock result =
			DriveInfoMock.New(driveName, FileSystem);

		await That(result.Name).IsEqualTo(expectedDriveName);
	}

	[Test]
	[AutoArguments]
	public async Task NotReady_AccessDirectory_ShouldThrowIOException(
		string path)
	{
		FileSystem.WithDrive(d => d.SetIsReady(false));

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.Directory.CreateDirectory(path);
		});

		await That(exception).IsExactly<IOException>();
	}

	[Test]
	[AutoArguments]
	public async Task NotReady_AccessFile_ShouldThrowIOException(
		string path, string contents)
	{
		FileSystem.File.WriteAllText(path, contents);
		FileSystem.WithDrive(d => d.SetIsReady(false));

		Exception? exception = Record.Exception(() =>
		{
			FileSystem.File.ReadAllText(path);
		});

		await That(exception).IsExactly<IOException>();
	}

	[Test]
	public async Task SetDriveFormat_Default_ShouldBeNTFS()
	{
		FileSystem.WithDrive(d => d.SetDriveFormat());

		IDriveInfo drive = FileSystem.GetDefaultDrive();
		await That(drive.DriveFormat).IsEqualTo("NTFS");
	}

	[Test]
	[AutoArguments]
	public async Task SetDriveFormat_ShouldChangeDriveFormat(string driveFormat)
	{
		FileSystem.WithDrive(d => d.SetDriveFormat(driveFormat));

		IDriveInfo drive = FileSystem.GetDefaultDrive();
		await That(drive.DriveFormat).IsEqualTo(driveFormat);
	}

	[Test]
	public async Task SetDriveType_Default_ShouldBeFixed()
	{
		FileSystem.WithDrive(d => d.SetDriveType());

		IDriveInfo drive = FileSystem.GetDefaultDrive();
		await That(drive.DriveType).IsEqualTo(DriveType.Fixed);
	}

	[Test]
	[AutoArguments]
	public async Task SetDriveType_ShouldChangeDriveType(DriveType driveType)
	{
		FileSystem.WithDrive(d => d.SetDriveType(driveType));

		IDriveInfo drive = FileSystem.GetDefaultDrive();
		await That(drive.DriveType).IsEqualTo(driveType);
	}

	[Test]
	[Arguments(true)]
	[Arguments(false)]
	public async Task SetIsReady_ShouldChangeIsReady(bool isReady)
	{
		FileSystem.WithDrive(d => d.SetIsReady(isReady));

		IDriveInfo drive = FileSystem.GetDefaultDrive();
		await That(drive.IsReady).IsEqualTo(isReady);
	}

	[Test]
	public async Task SetTotalSize_Default_ShouldBe1Gigabyte()
	{
		FileSystem.WithDrive(d => d.SetTotalSize());

		IDriveInfo drive = FileSystem.GetDefaultDrive();

		await That(drive.AvailableFreeSpace).IsEqualTo(1024 * 1024 * 1024);
	}
}
