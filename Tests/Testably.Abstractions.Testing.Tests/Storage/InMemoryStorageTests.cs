using System.IO;
using Testably.Abstractions.RandomSystem;
using Testably.Abstractions.Testing.Helpers;
using Testably.Abstractions.Testing.Storage;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Storage;

public class InMemoryStorageTests
{
	#region Test Setup

	internal MockFileSystem FileSystem { get; }
	internal IStorage Storage { get; }

	public InMemoryStorageTests()
	{
		FileSystem = new MockFileSystem();
		Storage = new InMemoryStorage(FileSystem);
	}

	#endregion

	[Theory]
	[AutoData]
	public void Copy_Overwrite_ShouldAdjustAvailableFreeSpace(
		int file1Size, int file2Size)
	{
		MockFileSystem fileSystem = new();
		IDriveInfo mainDrive = fileSystem.DriveInfo.New("".PrefixRoot());
		IRandom random = RandomFactory.Shared;
		byte[] file1Content = new byte[file1Size];
		byte[] file2Content = new byte[file2Size];
		random.NextBytes(file1Content);
		random.NextBytes(file2Content);

		fileSystem.File.WriteAllBytes("foo", file1Content);
		fileSystem.File.WriteAllBytes("bar", file2Content);
		long availableFreeSpaceBefore = mainDrive.AvailableFreeSpace;

		fileSystem.File.Copy("foo", "bar", true);

		long availableFreeSpaceAfter = mainDrive.AvailableFreeSpace;
		availableFreeSpaceAfter.Should()
			.Be(availableFreeSpaceBefore + file2Size - file1Size);
	}

	[Fact]
	public void CurrentDirectory_ShouldBeInitializedToDefaultRoot()
	{
		string expectedRoot = string.Empty.PrefixRoot();
		Storage.CurrentDirectory.Should().Be(expectedRoot);
	}

	[Fact]
	public void Delete_RaceCondition_ShouldReturnFalse()
	{
		MockFileSystem fileSystem = new();
		fileSystem.Directory.CreateDirectory("foo");
		bool isFirstDeletion = true;
		fileSystem.Intercept.Deleting(FileSystemTypes.Directory, _ =>
		{
			if (isFirstDeletion)
			{
				isFirstDeletion = false;
				fileSystem.Directory.Delete("foo");
			}
		});

		Exception? exception = Record.Exception(() =>
		{
			fileSystem.Directory.Delete("foo");
		});

		exception.Should().BeOfType<DirectoryNotFoundException>();
	}

	[Theory]
	[InlineData((string?)null)]
	[InlineData("")]
	public void GetDrive_NullOrEmpty_ShouldReturnNull(string? driveName)
	{
		IStorageDrive? result = Storage.GetDrive(driveName);

		result.Should().BeNull();
	}

	[Theory]
	[AutoData]
	public void Move_RequestDeniedForChild_ShouldRollback(
		string locationPath, string destinationPath)
	{
		IStorageLocation location = Storage.GetLocation(locationPath);
		IStorageLocation destination = Storage.GetLocation(destinationPath);
		IStorageLocation child1Location =
			Storage.GetLocation(Path.Combine(locationPath, "foo1"));
		IStorageLocation child2Location =
			Storage.GetLocation(Path.Combine(locationPath, "foo2"));
		LockableContainer lockedContainer = new(FileSystem);
		Storage.TryAddContainer(
			location,
			InMemoryContainer.NewDirectory,
			out _);
		Storage.TryAddContainer(
			child1Location,
			InMemoryContainer.NewFile,
			out _);
		Storage.TryAddContainer(
			child2Location,
			(_, _) => lockedContainer,
			out _);

		lockedContainer.IsLocked = true;

		Exception? exception = Record.Exception(() =>
		{
			Storage.Move(location, destination, recursive: true);
		});

		Storage.GetContainer(location).Should().NotBeOfType<NullContainer>();
		Storage.GetContainer(child1Location).Should().NotBeOfType<NullContainer>();
		Storage.GetContainer(child2Location).Should().NotBeOfType<NullContainer>();
		exception.Should().BeOfType<IOException>();
	}

	[Theory]
	[AutoData]
	public void Replace_WithBackup_ShouldChangeAvailableFreeSpace(
		int file1Size, int file2Size, int file3Size)
	{
		MockFileSystem fileSystem = new();
		IDriveInfo mainDrive = fileSystem.DriveInfo.New("".PrefixRoot());
		IRandom random = RandomFactory.Shared;
		byte[] file1Content = new byte[file1Size];
		byte[] file2Content = new byte[file2Size];
		byte[] file3Content = new byte[file3Size];
		random.NextBytes(file1Content);
		random.NextBytes(file2Content);
		random.NextBytes(file3Content);

		fileSystem.File.WriteAllBytes("foo", file1Content);
		fileSystem.File.WriteAllBytes("bar", file2Content);
		fileSystem.File.WriteAllBytes("backup", file3Content);
		long availableFreeSpaceBefore = mainDrive.AvailableFreeSpace;

		fileSystem.File.Replace("foo", "bar", "backup", true);

		long availableFreeSpaceAfter = mainDrive.AvailableFreeSpace;
		availableFreeSpaceAfter.Should()
			.Be(availableFreeSpaceBefore + file2Size);
	}

	[Theory]
	[AutoData]
	public void Replace_WithoutBackup_ShouldNotChangeAvailableFreeSpace(
		int file1Size, int file2Size)
	{
		MockFileSystem fileSystem = new();
		IDriveInfo mainDrive = fileSystem.DriveInfo.New("".PrefixRoot());
		IRandom random = RandomFactory.Shared;
		byte[] file1Content = new byte[file1Size];
		byte[] file2Content = new byte[file2Size];
		random.NextBytes(file1Content);
		random.NextBytes(file2Content);

		fileSystem.File.WriteAllBytes("foo", file1Content);
		fileSystem.File.WriteAllBytes("bar", file2Content);
		long availableFreeSpaceBefore = mainDrive.AvailableFreeSpace;

		fileSystem.File.Replace("foo", "bar", "backup");

		long availableFreeSpaceAfter = mainDrive.AvailableFreeSpace;
		availableFreeSpaceAfter.Should()
			.Be(availableFreeSpaceBefore);
	}

	[Theory]
	[AutoData]
	public void TryAddContainer_ShouldNotifyWhenAdded(string path)
	{
		bool receivedNotification = false;
		FileSystem.Notify.OnEvent(_ => receivedNotification = true);
		IStorageLocation location = Storage.GetLocation(path);
		bool result = Storage.TryAddContainer(
			location,
			InMemoryContainer.NewDirectory,
			out IStorageContainer? container);

		result.Should().BeTrue();
		receivedNotification.Should().BeTrue();
		container!.Type.Should().Be(FileSystemTypes.Directory);
	}

	[Theory]
	[AutoData]
	public void TryAddContainer_ShouldNotNotifyWhenExistsPreviously(string path)
	{
		IStorageLocation location = Storage.GetLocation(path);
		Storage.TryAddContainer(
			location,
			InMemoryContainer.NewDirectory,
			out _);
		bool receivedNotification = false;
		FileSystem.Notify.OnEvent(_ => receivedNotification = true);
		bool result = Storage.TryAddContainer(
			location,
			InMemoryContainer.NewDirectory,
			out IStorageContainer? container);

		result.Should().BeFalse();
		receivedNotification.Should().BeFalse();
		container.Should().BeNull();
	}
}
