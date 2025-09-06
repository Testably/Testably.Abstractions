using System.Globalization;
using System.IO;
using System.Linq;
using Testably.Abstractions.Testing.FileSystem;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing.Tests.Storage;

public class InMemoryContainerTests
{
	[Theory]
	[AutoData]
	public async Task AdjustAttributes_Decrypt_ShouldNotHaveEncryptedAttribute(string path)
	{
		MockFileSystem fileSystem = new();
		DriveInfoMock drive =
			DriveInfoMock.New("C", fileSystem);
		IStorageLocation location = InMemoryLocation.New(fileSystem, drive,
			fileSystem.Path.GetFullPath(path));

		InMemoryContainer container = new(FileSystemTypes.File, location, fileSystem);
		container.Encrypt();
		container.Decrypt();

		FileAttributes result = container.AdjustAttributes(FileAttributes.Normal);

		await That(result).DoesNotHaveFlag(FileAttributes.Encrypted);
	}

	[Theory]
	[AutoData]
	public async Task AdjustAttributes_Encrypt_ShouldHaveEncryptedAttribute(string path)
	{
		MockFileSystem fileSystem = new();
		DriveInfoMock drive =
			DriveInfoMock.New("C", fileSystem);
		IStorageLocation location = InMemoryLocation.New(fileSystem, drive,
			fileSystem.Path.GetFullPath(path));

		InMemoryContainer container = new(FileSystemTypes.File, location, fileSystem);
		container.Encrypt();

		FileAttributes result = container.AdjustAttributes(FileAttributes.Normal);

		await That(result).HasFlag(FileAttributes.Encrypted);
	}

	[Theory]
	[AutoData]
	public async Task AdjustAttributes_LeadingDot_ShouldBeHiddenOnLinux(string path)
	{
		path = "." + path;
		MockFileSystem fileSystem = new();
		DriveInfoMock drive =
			DriveInfoMock.New("C", fileSystem);
		IStorageLocation location = InMemoryLocation.New(fileSystem, drive,
			fileSystem.Path.GetFullPath(path));

		InMemoryContainer container = new(FileSystemTypes.File, location, fileSystem);

		FileAttributes result = container.AdjustAttributes(FileAttributes.Normal);

		if (Test.RunsOnLinux)
		{
			await That(result).HasFlag(FileAttributes.Hidden);
		}
		else
		{
			await That(result).DoesNotHaveFlag(FileAttributes.Hidden);
		}
	}

#if FEATURE_FILESYSTEM_LINK
	[Theory]
	[InlineAutoData(null, false)]
	[InlineAutoData("foo", true)]
	public async Task AdjustAttributes_ShouldHaveReparsePointAttributeWhenLinkTargetIsNotNull(
		string? linkTarget, bool shouldHaveReparsePoint, string path)
	{
		MockFileSystem fileSystem = new();
		DriveInfoMock drive =
			DriveInfoMock.New("C", fileSystem);
		IStorageLocation location = InMemoryLocation.New(fileSystem, drive,
			fileSystem.Path.GetFullPath(path));

		InMemoryContainer container = new(FileSystemTypes.File, location, fileSystem)
		{
			LinkTarget = linkTarget,
		};

		FileAttributes result = container.AdjustAttributes(FileAttributes.Normal);

		if (shouldHaveReparsePoint)
		{
			await That(result).HasFlag(FileAttributes.ReparsePoint);
		}
		else
		{
			await That(result).DoesNotHaveFlag(FileAttributes.ReparsePoint);
		}
	}
#endif

	[Theory]
	[AutoData]
	public async Task Container_ShouldProvideCorrectTimeAndFileSystem(string path)
	{
		MockFileSystem fileSystem = new();
		IStorageLocation location = InMemoryLocation.New(fileSystem, null, path);
		IStorageContainer sut = InMemoryContainer.NewFile(location, fileSystem);

		await That(sut.FileSystem).IsSameAs(fileSystem);
		await That(sut.TimeSystem).IsSameAs(fileSystem.TimeSystem);
	}

	[Theory]
	[AutoData]
	public async Task Decrypt_Encrypted_ShouldDecryptBytes(
		string path, byte[] bytes)
	{
		MockFileSystem fileSystem = new();
		DriveInfoMock drive =
			DriveInfoMock.New("C", fileSystem);
		IStorageLocation location = InMemoryLocation.New(fileSystem, drive,
			fileSystem.Path.GetFullPath(path));
		IStorageContainer fileContainer = InMemoryContainer.NewFile(location, fileSystem);
		fileContainer.WriteBytes(bytes);
		fileContainer.Encrypt();

		fileContainer.Decrypt();

		await That(fileContainer.Attributes).DoesNotHaveFlag(FileAttributes.Encrypted);
		await That(fileContainer.GetBytes()).IsEqualTo(bytes);
	}

	[Theory]
	[AutoData]
	public async Task Decrypt_Unencrypted_ShouldDoNothing(
		string path, byte[] bytes)
	{
		MockFileSystem fileSystem = new();
		DriveInfoMock drive =
			DriveInfoMock.New("C", fileSystem);
		IStorageLocation location = InMemoryLocation.New(fileSystem, drive,
			fileSystem.Path.GetFullPath(path));
		IStorageContainer fileContainer = InMemoryContainer.NewFile(location, fileSystem);
		fileContainer.WriteBytes(bytes);

		fileContainer.Decrypt();

		await That(fileContainer.GetBytes()).IsEqualTo(bytes);
	}

	[Theory]
	[AutoData]
	public async Task Encrypt_Encrypted_ShouldDoNothing(
		string path, byte[] bytes)
	{
		MockFileSystem fileSystem = new();
		DriveInfoMock drive =
			DriveInfoMock.New("C", fileSystem);
		IStorageLocation location = InMemoryLocation.New(fileSystem, drive,
			fileSystem.Path.GetFullPath(path));
		IStorageContainer fileContainer = InMemoryContainer.NewFile(location, fileSystem);
		fileContainer.WriteBytes(bytes);
		fileContainer.Encrypt();

		fileContainer.Encrypt();

		fileContainer.Decrypt();
		await That(fileContainer.GetBytes()).IsEqualTo(bytes);
	}

	[Theory]
	[AutoData]
	public async Task Encrypt_ShouldEncryptBytes(
		string path, byte[] bytes)
	{
		MockFileSystem fileSystem = new();
		DriveInfoMock drive =
			DriveInfoMock.New("C", fileSystem);
		IStorageLocation location = InMemoryLocation.New(fileSystem, drive,
			fileSystem.Path.GetFullPath(path));
		IStorageContainer fileContainer = InMemoryContainer.NewFile(location, fileSystem);
		fileContainer.WriteBytes(bytes);

		fileContainer.Encrypt();

		await That(fileContainer.Attributes).HasFlag(FileAttributes.Encrypted);
		await That(fileContainer.GetBytes()).IsNotEqualTo(bytes);
	}

	[Theory]
	[AutoData]
	public async Task RequestAccess_ToString_DeleteAccess_ShouldContainAccessAndShare(string path,
		FileAccess access, FileShare share)
	{
		MockFileSystem fileSystem = new();
		fileSystem.Initialize()
			.WithFile(path);
		IStorageLocation location = fileSystem.Storage.GetLocation(path);
		IStorageContainer sut = InMemoryContainer.NewFile(location, fileSystem);

		IStorageAccessHandle result = sut.RequestAccess(access, share, true);

		await That(result.ToString()).DoesNotContain(access.ToString());
		await That(result.ToString()).Contains("Delete");
		await That(result.ToString()).Contains(share.ToString());
	}

	[Theory]
	[AutoData]
	public async Task RequestAccess_ToString_ShouldContainAccessAndShare(string path,
		FileAccess access,
		FileShare share)
	{
		MockFileSystem fileSystem = new();
		fileSystem.Initialize()
			.WithFile(path);
		IStorageLocation location = fileSystem.Storage.GetLocation(path);
		IStorageContainer sut = InMemoryContainer.NewFile(location, fileSystem);

		IStorageAccessHandle result = sut.RequestAccess(access, share);

		await That(result.ToString()).Contains(access.ToString());
		await That(result.ToString()).Contains(share.ToString());
	}

	[Theory]
	[AutoData]
	public async Task RequestAccess_WithoutDrive_ShouldThrowDirectoryNotFoundException(
		string path)
	{
		MockFileSystem fileSystem = new();
		IStorageLocation location = InMemoryLocation.New(fileSystem, null, path);
		IStorageContainer fileContainer = InMemoryContainer.NewFile(location, fileSystem);

		Exception? exception = Record.Exception(() =>
		{
			fileContainer.RequestAccess(FileAccess.Read, FileShare.Read);
		});

		await That(exception).IsExactly<DirectoryNotFoundException>();
	}

	[Theory]
	[InlineAutoData(DateTimeKind.Local)]
	[InlineAutoData(DateTimeKind.Utc)]
	public async Task TimeContainer_Time_Set_WithUnspecifiedKind_ShouldSetToProvidedKind(
		DateTimeKind kind, string path, DateTime time)
	{
		time = DateTime.SpecifyKind(time, DateTimeKind.Unspecified);
		MockFileSystem fileSystem = new();
		IStorageLocation location = InMemoryLocation.New(fileSystem, null, path);
		IStorageContainer fileContainer = InMemoryContainer.NewFile(location, fileSystem);

		fileContainer.CreationTime.Set(time, kind);

		DateTime result = fileContainer.CreationTime.Get(DateTimeKind.Unspecified);

		await That(result).IsEqualTo(time);
		await That(result.Kind).IsEqualTo(kind);
	}

	[Theory]
	[AutoData]
	public async Task TimeContainer_ToString_ShouldReturnUtcTime(
		string path, DateTime time)
	{
		time = DateTime.SpecifyKind(time, DateTimeKind.Local);
		string expectedString = time.ToUniversalTime()
			.ToString("yyyy-MM-dd HH:mm:ssZ", CultureInfo.InvariantCulture);
		MockFileSystem fileSystem = new();
		IStorageLocation location = InMemoryLocation.New(fileSystem, null, path);
		IStorageContainer fileContainer = InMemoryContainer.NewFile(location, fileSystem);

		fileContainer.CreationTime.Set(time, DateTimeKind.Unspecified);

		string? result = fileContainer.CreationTime.ToString();

		await That(result).IsEqualTo(expectedString);
	}

	[Fact]
	public async Task ToString_Directory_ShouldIncludePath()
	{
		MockFileSystem fileSystem = new();
		string expectedPath = fileSystem.Path.GetFullPath("foo");
		fileSystem.Directory.CreateDirectory(expectedPath);
		#pragma warning disable CA1826
		IStorageContainer sut = fileSystem.StorageContainers.Last();
		#pragma warning restore CA1826

		string? result = sut.ToString();

		await That(result).IsEqualTo($"{expectedPath}: Directory");
	}

	[Theory]
	[AutoData]
	public async Task ToString_File_ShouldIncludePathAndFileSize(byte[] bytes)
	{
		MockFileSystem fileSystem = new();
		string expectedPath = fileSystem.Path.GetFullPath("foo.txt");
		fileSystem.File.WriteAllBytes(expectedPath, bytes);
		IStorageContainer sut = fileSystem.StorageContainers
			.Single(x => x.Type == FileSystemTypes.File);

		string? result = sut.ToString();

		await That(result).IsEqualTo($"{expectedPath}: File ({bytes.Length} bytes)");
	}

	[Fact]
	public async Task ToString_UnknownContainer_ShouldIncludePath()
	{
		MockFileSystem fileSystem = new();
		string expectedPath = fileSystem.Path.GetFullPath("foo");
		IStorageLocation location = InMemoryLocation.New(fileSystem, null, expectedPath);
		InMemoryContainer sut = new(FileSystemTypes.DirectoryOrFile, location,
			fileSystem);

		string result = sut.ToString();

		await That(result).IsEqualTo($"{expectedPath}: Unknown Container");
	}
}
