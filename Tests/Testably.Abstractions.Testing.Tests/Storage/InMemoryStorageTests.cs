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

	[Test]
	[AutoArguments]
	public async Task Copy_Overwrite_ShouldAdjustAvailableFreeSpace(
		int file1Size, int file2Size)
	{
		MockFileSystem fileSystem = new();
		IDriveInfo mainDrive = fileSystem.DriveInfo.New("".PrefixRoot(fileSystem));
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
		await That(availableFreeSpaceAfter)
			.IsEqualTo(availableFreeSpaceBefore + file2Size - file1Size);
	}

#if FEATURE_FILESYSTEM_HARDLINK
	[Test]
	public async Task CreateHardLink_OnDifferentDrive_ShouldThrowIOException()
	{
		MockFileSystem fileSystem = new(o => o.SimulatingOperatingSystem(SimulationMode.Windows));
		fileSystem.WithUncDrive(@"\\server");
		string path = @"\\server\link.txt";
		fileSystem.File.WriteAllText("target.txt", "foo");

		void Act()
		{
			fileSystem.File.CreateHardLink(path, "target.txt");
		}

		await That(Act).Throws<IOException>()
			.WithMessage($"The system cannot move the file to a different disk drive. : '{path}'").And
			.WithHResult(-2147024879);
	}
#endif

#if FEATURE_FILESYSTEM_HARDLINK
	[Test]
	[AutoArguments]
	public async Task CreateHardLink_ShouldOnlyReleaseUsedSpaceWhenLastLinkIsDeleted(byte[] bytes)
	{
		MockFileSystem fileSystem = new();
		IDriveInfo mainDrive = fileSystem.DriveInfo.New("".PrefixRoot(fileSystem));
		long availableFreeSpaceInitially = mainDrive.AvailableFreeSpace;
		fileSystem.File.WriteAllBytes("target", bytes);

		fileSystem.File.CreateHardLink("link", "target");
		long availableFreeSpaceWithLink = mainDrive.AvailableFreeSpace;
		fileSystem.File.Delete("target");
		long availableFreeSpaceAfterFirstDelete = mainDrive.AvailableFreeSpace;
		fileSystem.File.Delete("link");

		await That(availableFreeSpaceWithLink)
			.IsEqualTo(availableFreeSpaceInitially - bytes.Length)
			.Because("both links share the same content");
		await That(availableFreeSpaceAfterFirstDelete).IsEqualTo(availableFreeSpaceWithLink);
		await That(mainDrive.AvailableFreeSpace).IsEqualTo(availableFreeSpaceInitially);
	}
#endif

	[Test]
	public async Task CurrentDirectory_ShouldBeInitializedToDefaultRoot()
	{
		string expectedRoot = string.Empty.PrefixRoot(new MockFileSystem());
		await That(Storage.CurrentDirectory).IsEqualTo(expectedRoot);
	}

	[Test]
	public async Task Delete_RaceCondition_ShouldReturnFalse()
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

		await That(exception).IsExactly<DirectoryNotFoundException>();
	}

	[Test]
	[Arguments((string?)null)]
	[Arguments("")]
	[Arguments(" ")]
	[Arguments("\t")]
	public async Task GetDrive_NullOrWhitespace_ShouldReturnNull(string? driveName)
	{
		IStorageDrive? result = Storage.GetDrive(driveName);

		await That(result).IsNull();
	}

	[Test]
	public async Task GetOrAddDrive_Null_ShouldReturnNull()
	{
		IStorageDrive? result = Storage.GetOrAddDrive(driveName: null);

		await That(result).IsNull();
	}

	[Test]
	public async Task GetOrCreateContainer_WithMetadata_ShouldBeKept()
	{
		FileSystemExtensibility extensibility = new();
		extensibility.StoreMetadata("foo1", "bar1");
		extensibility.StoreMetadata("foo2", 42);

		IStorageContainer container = Storage.GetOrCreateContainer(
			Storage.GetLocation("foo"),
			(location, fileSystem) => new InMemoryContainer(
				FileSystemTypes.File, location, fileSystem),
			extensibility);

		string? result1 = container.Extensibility.RetrieveMetadata<string>("foo1");
		await That(result1).IsEqualTo("bar1");
		int result2 = container.Extensibility.RetrieveMetadata<int>("foo2");
		await That(result2).IsEqualTo(42);
	}

	[Test]
	[AutoArguments]
	public async Task Move_RequestDeniedForChild_ShouldRollback(
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

		await That(Storage.GetContainer(location)).IsNot<NullContainer>();
		await That(Storage.GetContainer(child1Location)).IsNot<NullContainer>();
		await That(Storage.GetContainer(child2Location)).IsNot<NullContainer>();
		await That(exception).IsExactly<IOException>();
	}

	[Test]
	[AutoArguments]
	public async Task Replace_WithBackup_ShouldChangeAvailableFreeSpace(
		int file1Size, int file2Size, int file3Size)
	{
		MockFileSystem fileSystem = new();
		IDriveInfo mainDrive = fileSystem.DriveInfo.New("".PrefixRoot(fileSystem));
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
		await That(availableFreeSpaceAfter).IsEqualTo(availableFreeSpaceBefore + file2Size);
	}

	[Test]
	[AutoArguments]
	public async Task Replace_WithoutBackup_ShouldNotChangeAvailableFreeSpace(
		int file1Size, int file2Size)
	{
		MockFileSystem fileSystem = new();
		IDriveInfo mainDrive = fileSystem.DriveInfo.New("".PrefixRoot(fileSystem));
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
		await That(availableFreeSpaceAfter).IsEqualTo(availableFreeSpaceBefore);
	}

	[Test]
	[AutoArguments]
	public async Task TryAddContainer_ShouldNotifyWhenAdded(string path)
	{
		bool receivedNotification = false;
		FileSystem.Notify.OnEvent(_ => receivedNotification = true);
		IStorageLocation location = Storage.GetLocation(path);
		bool result = Storage.TryAddContainer(
			location,
			InMemoryContainer.NewDirectory,
			out IStorageContainer? container);

		await That(result).IsTrue();
		await That(receivedNotification).IsTrue();
		await That(container!.Type).IsEqualTo(FileSystemTypes.Directory);
	}

	[Test]
	[AutoArguments]
	public async Task TryAddContainer_ShouldNotNotifyWhenExistsPreviously(string path)
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

		await That(result).IsFalse();
		await That(receivedNotification).IsFalse();
		await That(container).IsNull();
	}
}
