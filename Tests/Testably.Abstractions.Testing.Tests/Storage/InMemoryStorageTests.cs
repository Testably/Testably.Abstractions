using System.IO;
using Testably.Abstractions.Testing.Internal;
using Testably.Abstractions.Testing.Storage;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Storage;

public class InMemoryStorageTests
{
	#region Test Setup

	internal Testing.FileSystemMock FileSystem { get; }
	internal IStorage Storage { get; }

	public InMemoryStorageTests()
	{
		FileSystem = new Testing.FileSystemMock();
		Storage = new InMemoryStorage(FileSystem);
	}

	#endregion

	[Fact]
	[Trait(nameof(Testing), nameof(InMemoryStorage))]
	public void CurrentDirectory_ShouldBeInitializedToDefaultRoot()
	{
		string expectedRoot = string.Empty.PrefixRoot();
		Storage.CurrentDirectory.Should().Be(expectedRoot);
	}

	[Theory]
	[AutoData]
	[Trait(nameof(Testing), nameof(InMemoryStorage))]
	public void TryAddContainer_ShouldNotifyWhenAdded(string path)
	{
		bool receivedNotification = false;
		FileSystem.Notify.OnChange(_ => receivedNotification = true);
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
	[Trait(nameof(Testing), nameof(InMemoryStorage))]
	public void TryAddContainer_ShouldNotNotifyWhenExistsPreviously(string path)
	{
		IStorageLocation location = Storage.GetLocation(path);
		Storage.TryAddContainer(
			location,
			InMemoryContainer.NewDirectory,
			out _);
		bool receivedNotification = false;
		FileSystem.Notify.OnChange(_ => receivedNotification = true);
		bool result = Storage.TryAddContainer(
			location,
			InMemoryContainer.NewDirectory,
			out IStorageContainer? container);

		result.Should().BeFalse();
		receivedNotification.Should().BeFalse();
		container.Should().BeNull();
	}

	[Theory]
	[AutoData]
	[Trait(nameof(Testing), nameof(InMemoryStorage))]
	public void Move_RequestDeniedForChild_ShouldRollback(
		string locationPath, string destinationPath)
	{
		IStorageLocation location = Storage.GetLocation(locationPath);
		IStorageLocation destination = Storage.GetLocation(destinationPath);
		IStorageLocation child1Location =
			Storage.GetLocation(Path.Combine(locationPath, "foo"));
		IStorageLocation child2Location =
			Storage.GetLocation(Path.Combine(locationPath, "bar"));
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
}