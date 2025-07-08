using System.IO;
using Testably.Abstractions.Testing.Storage;

namespace Testably.Abstractions.Testing.Tests.Storage;

public class NullContainerTests
{
	[Theory]
	[AutoData]
	public async Task AppendBytes_ShouldReturnEmptyArray(byte[] bytes)
	{
		MockFileSystem fileSystem = new();
		IStorageContainer sut = NullContainer.New(fileSystem);

		sut.AppendBytes(bytes);

		await That(sut.GetBytes()).IsEmpty();
	}

	[Fact]
	public async Task ClearBytes_ShouldReturnEmptyArray()
	{
		MockFileSystem fileSystem = new();
		IStorageContainer sut = NullContainer.New(fileSystem);

		sut.ClearBytes();

		await That(sut.GetBytes()).IsEmpty();
	}

	[Fact]
	public async Task Constructor_ShouldSetFileAndTimeSystem()
	{
		MockFileSystem fileSystem = new();

		IStorageContainer sut = NullContainer.New(fileSystem);

		await That(sut.FileSystem).IsSameAs(fileSystem);
		await That(sut.TimeSystem).IsSameAs(fileSystem.TimeSystem);
	}

	[Fact]
	public async Task CreationTime_WithUnspecifiedKind_ShouldReturnNullTime()
	{
		MockFileSystem fileSystem = new();
		IStorageContainer sut = NullContainer.New(fileSystem);

		DateTime result = sut.CreationTime.Get(DateTimeKind.Unspecified);

		await That(result).IsEqualTo(new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc));
	}

	[Fact]
	public async Task Decrypt_ShouldReturnEmptyArray()
	{
		MockFileSystem fileSystem = new();
		IStorageContainer sut = NullContainer.New(fileSystem);

		sut.Decrypt();

		await That(sut.GetBytes()).IsEmpty();
	}

	[Fact]
	public async Task Encrypt_ShouldReturnEmptyArray()
	{
		MockFileSystem fileSystem = new();
		IStorageContainer sut = NullContainer.New(fileSystem);

		sut.Encrypt();

		await That(sut.GetBytes()).IsEmpty();
	}

	[Fact]
	public async Task GetBytes_ShouldReturnEmptyArray()
	{
		MockFileSystem fileSystem = new();
		IStorageContainer sut = NullContainer.New(fileSystem);

		await That(sut.GetBytes()).IsEmpty();
	}

	[Theory]
	[AutoData]
	public async Task LinkTarget_ShouldAlwaysReturnNull(string linkTarget)
	{
		MockFileSystem fileSystem = new();
		IStorageContainer sut = NullContainer.New(fileSystem);
		await That(sut.LinkTarget).IsNull();

		sut.LinkTarget = linkTarget;

		await That(sut.LinkTarget).IsNull();
	}

	[Theory]
	[AutoData]
	public async Task RequestAccess_Dispose_Twice_ShouldDoNothing(FileAccess access, FileShare share)
	{
		MockFileSystem fileSystem = new();
		IStorageContainer sut = NullContainer.New(fileSystem);

		IStorageAccessHandle result = sut.RequestAccess(access, share);
		result.Dispose();

		Exception? exception = Record.Exception(() =>
		{
			result.Dispose();
		});

		await That(exception).IsNull();
	}

	[Theory]
	[AutoData]
	public async Task RequestAccess_ShouldReturnNullObject(FileAccess access, FileShare share,
		bool deleteAccess)
	{
		MockFileSystem fileSystem = new();
		IStorageContainer sut = NullContainer.New(fileSystem);

		IStorageAccessHandle result = sut.RequestAccess(access, share, deleteAccess);

		await That(result.Access).IsEqualTo(access);
		await That(result.Share).IsEqualTo(share);
		await That(result.DeleteAccess).IsEqualTo(deleteAccess);
	}

	[Fact]
	public async Task Type_ShouldBeDirectoryOrFile()
	{
		MockFileSystem fileSystem = new();
		IStorageContainer sut = NullContainer.New(fileSystem);

		await That(sut.Type).IsEqualTo(FileSystemTypes.DirectoryOrFile);
	}

	[Theory]
	[AutoData]
	public async Task WriteBytes_ShouldReturnEmptyArray(byte[] bytes)
	{
		MockFileSystem fileSystem = new();
		IStorageContainer sut = NullContainer.New(fileSystem);

		sut.WriteBytes(bytes);

		await That(sut.GetBytes()).IsEmpty();
	}
}
