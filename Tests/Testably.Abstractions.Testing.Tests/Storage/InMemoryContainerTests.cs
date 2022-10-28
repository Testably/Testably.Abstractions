using System.IO;
using Testably.Abstractions.Testing.FileSystem;
using Testably.Abstractions.Testing.Storage;
using Testably.Abstractions.Testing.Tests.TestHelpers;

namespace Testably.Abstractions.Testing.Tests.Storage;

public class InMemoryContainerTests
{
	[Theory]
	[AutoData]
	public void AdjustAttributes_Decrypt_ShouldNotHaveEncryptedAttribute(string path)
	{
		MockFileSystem fileSystem = new();
		DriveInfoMock drive =
			DriveInfoMock.New("C", fileSystem);
		IStorageLocation location = InMemoryLocation.New(drive,
			fileSystem.Path.GetFullPath(path));

		InMemoryContainer container = new(FileSystemTypes.File, location, fileSystem);
		container.Encrypt();
		container.Decrypt();

		FileAttributes result = container.AdjustAttributes(FileAttributes.Normal);

		result.Should().NotHaveFlag(FileAttributes.Encrypted);
	}

	[Theory]
	[AutoData]
	public void AdjustAttributes_Encrypt_ShouldHaveEncryptedAttribute(string path)
	{
		MockFileSystem fileSystem = new();
		DriveInfoMock drive =
			DriveInfoMock.New("C", fileSystem);
		IStorageLocation location = InMemoryLocation.New(drive,
			fileSystem.Path.GetFullPath(path));

		InMemoryContainer container = new(FileSystemTypes.File, location, fileSystem);
		container.Encrypt();

		FileAttributes result = container.AdjustAttributes(FileAttributes.Normal);

		result.Should().HaveFlag(FileAttributes.Encrypted);
	}

	[Theory]
	[AutoData]
	public void AdjustAttributes_LeadingDot_ShouldBeHiddenOnLinux(string path)
	{
		path = "." + path;
		MockFileSystem fileSystem = new();
		DriveInfoMock drive =
			DriveInfoMock.New("C", fileSystem);
		IStorageLocation location = InMemoryLocation.New(drive,
			fileSystem.Path.GetFullPath(path));

		InMemoryContainer container = new(FileSystemTypes.File, location, fileSystem);

		FileAttributes result = container.AdjustAttributes(FileAttributes.Normal);

		if (Test.RunsOnLinux)
		{
			result.Should().HaveFlag(FileAttributes.Hidden);
		}
		else
		{
			result.Should().NotHaveFlag(FileAttributes.Hidden);
		}
	}

#if FEATURE_FILESYSTEM_LINK
	[Theory]
	[InlineAutoData(null, false)]
	[InlineAutoData("foo", true)]
	public void AdjustAttributes_ShouldHaveReparsePointAttributeWhenLinkTargetIsNotNull(
		string? linkTarget, bool shouldHaveReparsePoint, string path)
	{
		MockFileSystem fileSystem = new();
		DriveInfoMock drive =
			DriveInfoMock.New("C", fileSystem);
		IStorageLocation location = InMemoryLocation.New(drive,
			fileSystem.Path.GetFullPath(path));

		InMemoryContainer container = new(FileSystemTypes.File, location, fileSystem);
		container.LinkTarget = linkTarget;

		FileAttributes result = container.AdjustAttributes(FileAttributes.Normal);

		if (shouldHaveReparsePoint)
		{
			result.Should().HaveFlag(FileAttributes.ReparsePoint);
		}
		else
		{
			result.Should().NotHaveFlag(FileAttributes.ReparsePoint);
		}
	}
#endif

	[Theory]
	[AutoData]
	public void Decrypt_Encrypted_ShouldDecryptBytes(
		string path, byte[] bytes)
	{
		MockFileSystem fileSystem = new();
		DriveInfoMock drive =
			DriveInfoMock.New("C", fileSystem);
		IStorageLocation location = InMemoryLocation.New(drive,
			fileSystem.Path.GetFullPath(path));
		IStorageContainer fileContainer = InMemoryContainer.NewFile(location, fileSystem);
		fileContainer.WriteBytes(bytes);
		fileContainer.Encrypt();

		fileContainer.Decrypt();

		fileContainer.Attributes.Should().NotHaveFlag(FileAttributes.Encrypted);
		fileContainer.GetBytes().Should().BeEquivalentTo(bytes);
	}

	[Theory]
	[AutoData]
	public void Decrypt_Unencrypted_ShouldDoNothing(
		string path, byte[] bytes)
	{
		MockFileSystem fileSystem = new();
		DriveInfoMock drive =
			DriveInfoMock.New("C", fileSystem);
		IStorageLocation location = InMemoryLocation.New(drive,
			fileSystem.Path.GetFullPath(path));
		IStorageContainer fileContainer = InMemoryContainer.NewFile(location, fileSystem);
		fileContainer.WriteBytes(bytes);

		fileContainer.Decrypt();

		fileContainer.GetBytes().Should().BeEquivalentTo(bytes);
	}

	[Theory]
	[AutoData]
	public void Encrypt_Encrypted_ShouldDoNothing(
		string path, byte[] bytes)
	{
		MockFileSystem fileSystem = new();
		DriveInfoMock drive =
			DriveInfoMock.New("C", fileSystem);
		IStorageLocation location = InMemoryLocation.New(drive,
			fileSystem.Path.GetFullPath(path));
		IStorageContainer fileContainer = InMemoryContainer.NewFile(location, fileSystem);
		fileContainer.WriteBytes(bytes);
		fileContainer.Encrypt();

		fileContainer.Encrypt();

		fileContainer.Decrypt();
		fileContainer.GetBytes().Should().BeEquivalentTo(bytes);
	}

	[Theory]
	[AutoData]
	public void Encrypt_ShouldEncryptBytes(
		string path, byte[] bytes)
	{
		MockFileSystem fileSystem = new();
		DriveInfoMock drive =
			DriveInfoMock.New("C", fileSystem);
		IStorageLocation location = InMemoryLocation.New(drive,
			fileSystem.Path.GetFullPath(path));
		IStorageContainer fileContainer = InMemoryContainer.NewFile(location, fileSystem);
		fileContainer.WriteBytes(bytes);

		fileContainer.Encrypt();

		fileContainer.Attributes.Should().HaveFlag(FileAttributes.Encrypted);
		fileContainer.GetBytes().Should().NotBeEquivalentTo(bytes);
	}

	[Theory]
	[AutoData]
	public void RequestAccess_WithoutDrive_ShouldThrowDirectoryNotFoundException(
		string path)
	{
		MockFileSystem fileSystem = new();
		IStorageLocation location = InMemoryLocation.New(null, path);
		IStorageContainer fileContainer = InMemoryContainer.NewFile(location, fileSystem);

		Exception? exception = Record.Exception(() =>
		{
			fileContainer.RequestAccess(FileAccess.Read, FileShare.Read);
		});

		exception.Should().BeOfType<DirectoryNotFoundException>();
	}

	[Theory]
	[InlineAutoData(DateTimeKind.Local)]
	[InlineAutoData(DateTimeKind.Utc)]
	public void TimeContainer_Time_Set_WithUnspecifiedKind_ShouldSetToProvidedKind(
		DateTimeKind kind, string path, DateTime time)
	{
		time = DateTime.SpecifyKind(time, DateTimeKind.Unspecified);
		MockFileSystem fileSystem = new();
		IStorageLocation location = InMemoryLocation.New(null, path);
		IStorageContainer fileContainer = InMemoryContainer.NewFile(location, fileSystem);

		fileContainer.CreationTime.Set(time, kind);

		DateTime result = fileContainer.CreationTime.Get(DateTimeKind.Unspecified);

		result.Should().Be(time);
		result.Kind.Should().Be(kind);
	}
}