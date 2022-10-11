using Testably.Abstractions.Testing.Internal;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing.Tests.Storage;

public class InMemoryStorageTests
{
	#region Test Setup

	internal FileSystemMock FileSystem { get; }
	internal IStorage Storage { get; }

	public InMemoryStorageTests()
	{
		FileSystem = new FileSystemMock();
		Storage = new InMemoryStorage(FileSystem);
	}

	#endregion

	[Fact]
	public void CurrentDirectory_ShouldBeInitializedToDefaultRoot()
	{
		string expectedRoot = string.Empty.PrefixRoot();
		Storage.CurrentDirectory.Should().Be(expectedRoot);
	}

	[Theory]
	[AutoData]
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
		container!.Type.Should().Be(ContainerTypes.Directory);
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
		FileSystem.Notify.OnChange(_ => receivedNotification = true);
		bool result = Storage.TryAddContainer(
			location,
			InMemoryContainer.NewDirectory,
			out IStorageContainer? container);

		result.Should().BeFalse();
		receivedNotification.Should().BeFalse();
		container.Should().BeNull();
	}
}