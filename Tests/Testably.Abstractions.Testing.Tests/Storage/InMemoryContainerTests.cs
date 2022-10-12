using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing.Tests.Storage;

public class InMemoryContainerTests
{
	[Theory]
	[AutoData]
	[Trait(nameof(Testing), nameof(InMemoryContainer))]
	public void Decrypt_Encrypted_ShouldDecryptBytes(
		string path, byte[] bytes)
	{
		FileSystemMock fileSystem = new();
		FileSystemMock.DriveInfoMock drive =
			FileSystemMock.DriveInfoMock.New("C", fileSystem);
		IStorageLocation location = InMemoryLocation.New(drive, path);
		IStorageContainer fileContainer = InMemoryContainer.NewFile(location, fileSystem);
		fileContainer.WriteBytes(bytes);
		fileContainer.Encrypt();

		fileContainer.Decrypt();

		fileContainer.GetBytes().Should().BeEquivalentTo(bytes);
	}

	[Theory]
	[AutoData]
	[Trait(nameof(Testing), nameof(InMemoryContainer))]
	public void Decrypt_Unencrypted_ShouldDoNothing(
		string path, byte[] bytes)
	{
		FileSystemMock fileSystem = new();
		FileSystemMock.DriveInfoMock drive =
			FileSystemMock.DriveInfoMock.New("C", fileSystem);
		IStorageLocation location = InMemoryLocation.New(drive, path);
		IStorageContainer fileContainer = InMemoryContainer.NewFile(location, fileSystem);
		fileContainer.WriteBytes(bytes);

		fileContainer.Decrypt();

		fileContainer.GetBytes().Should().BeEquivalentTo(bytes);
	}

	[Theory]
	[AutoData]
	[Trait(nameof(Testing), nameof(InMemoryContainer))]
	public void Encrypt_Encrypted_ShouldDoNothing(
		string path, byte[] bytes)
	{
		FileSystemMock fileSystem = new();
		FileSystemMock.DriveInfoMock drive =
			FileSystemMock.DriveInfoMock.New("C", fileSystem);
		IStorageLocation location = InMemoryLocation.New(drive, path);
		IStorageContainer fileContainer = InMemoryContainer.NewFile(location, fileSystem);
		fileContainer.WriteBytes(bytes);
		fileContainer.Encrypt();

		fileContainer.Encrypt();

		fileContainer.Decrypt();
		fileContainer.GetBytes().Should().BeEquivalentTo(bytes);
	}
}