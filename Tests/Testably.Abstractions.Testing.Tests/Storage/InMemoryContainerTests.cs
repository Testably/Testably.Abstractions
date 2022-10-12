﻿using System.IO;
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

		fileContainer.Attributes.Should().NotHaveFlag(FileAttributes.Encrypted);
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

	[Theory]
	[AutoData]
	[Trait(nameof(Testing), nameof(InMemoryContainer))]
	public void Encrypt_ShouldEncryptBytes(
		string path, byte[] bytes)
	{
		FileSystemMock fileSystem = new();
		FileSystemMock.DriveInfoMock drive =
			FileSystemMock.DriveInfoMock.New("C", fileSystem);
		IStorageLocation location = InMemoryLocation.New(drive, path);
		IStorageContainer fileContainer = InMemoryContainer.NewFile(location, fileSystem);
		fileContainer.WriteBytes(bytes);

		fileContainer.Encrypt();

		fileContainer.Attributes.Should().HaveFlag(FileAttributes.Encrypted);
		fileContainer.GetBytes().Should().NotBeEquivalentTo(bytes);
	}

	[Theory]
	[AutoData]
	[Trait(nameof(Testing), nameof(InMemoryContainer))]
	public void RequestAccess_WithoutDrive_ShouldThrowDirectoryNotFoundException(
		string path)
	{
		FileSystemMock fileSystem = new();
		IStorageLocation location = InMemoryLocation.New(null, path);
		IStorageContainer fileContainer = InMemoryContainer.NewFile(location, fileSystem);

		Exception? exception = Record.Exception(() =>
		{
			fileContainer.RequestAccess(FileAccess.Read, FileShare.Read);
		});

		exception.Should().BeOfType<DirectoryNotFoundException>();
	}
}